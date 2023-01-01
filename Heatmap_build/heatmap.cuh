#pragma once

#include <device_launch_parameters.h>
#include <cuda_runtime_api.h>
#include <cstdint>
#include <stdio.h>

#define NEW_BLOCK 0XFFFFFFFF
#define BLOCK_SIZE 512.0
#define UNREACHABLE 1
#define REACHABLE 0

static __global__ 
void fill_nextlayer(std::uint32_t *map, std::uint32_t size, std::uint32_t *curq, std::uint32_t n, std::uint32_t *deg, std::uint32_t *heatmap, std::uint32_t *parent, std::uint32_t length, std::uint32_t level)
{
  std::uint32_t id = threadIdx.x + blockDim.x * blockIdx.x;
  if (id < n)
  {
    /* Check for u's neighbour */
    std::uint32_t u = curq[id], v = u - 1, d = 0;
    if (u % length > 0 && map[v] != UNREACHABLE)
      if (atomicCAS(&parent[v], NEW_BLOCK, u) == NEW_BLOCK)
      { 
        heatmap[v] = level;
        d++;
      }
    v = u + 1;
    if (u % length < length - 1 && map[v] != UNREACHABLE)
      if (atomicCAS(&parent[v], NEW_BLOCK, u) == NEW_BLOCK)
      { 
        heatmap[v] = level;
        d++;
      }
    v = u - length;
    if (u >= length && map[v] != UNREACHABLE)
      if (atomicCAS(&parent[v], NEW_BLOCK, u) == NEW_BLOCK)
      { 
        heatmap[v] = level;
        d++;
      }
    v = u + length;
    if (u + length < size && map[v] != UNREACHABLE)
      if (atomicCAS(&parent[v], NEW_BLOCK, u) == NEW_BLOCK)
      { 
        heatmap[v] = level;
        d++;
      }
    deg[id] = d;
  }
}

/* Parallel prefix sum (work for n <= BLOCK_SIZE) */
static __global__
void in_scan(std::uint32_t *arr, std::uint32_t n, std::uint32_t *aux_arr)
{
  std::uint32_t id = threadIdx.x + blockDim.x * blockIdx.x;
  if (id < n)
  {
    std::uint32_t t = threadIdx.x;
    __shared__ std::uint32_t bound;
    __shared__ std::uint32_t s_arr[(size_t)BLOCK_SIZE];
    s_arr[t] = arr[id];
    if (t == blockDim.x - 1 || id == n - 1)
      bound = t + 1;
    __syncthreads();
    std::uint32_t b = bound;
    
    std::uint32_t stride = 1;
    while (stride <= (b >> 1))
    {
      std::uint32_t i = (stride << 1) * (t + 1) - 1;
      if (i < b)
        s_arr[i] += s_arr[i - stride];
      stride <<= 1;
      __syncthreads();
    }
    stride >>= 1;
    while (stride > 0)
    {
      std::uint32_t i = (stride << 1) * (t + 1) - 1;
      if (i + stride < b)
        s_arr[i + stride] += s_arr[i];
      stride >>= 1;
      __syncthreads();
    }

    if (t == b - 1 && aux_arr != nullptr)
    {
      aux_arr[blockIdx.x] = s_arr[t];
    }
    arr[id] = s_arr[t];
  }
}

static __global__
void block_add(std::uint32_t *arr, std::uint32_t n, std::uint32_t *aux_arr)
{
  std::uint32_t id = threadIdx.x + blockDim.x * blockIdx.x;
  if (id < n && blockIdx.x > 0)
  {  
    arr[id] += aux_arr[blockIdx.x - 1];
  }
}

/* Set queue in next layer */
static __global__
void assign_nextq(std::uint32_t size, std::uint32_t *curq, std::uint32_t n, std::uint32_t *nextq, std::uint32_t *deg, std::uint32_t *parent, std::uint32_t length)
{
  std::uint32_t id = threadIdx.x + blockDim.x * blockIdx.x;
  if (id < n)
  {
    std::uint32_t start, u = curq[id], v = u - 1, c = 0;
    if (id > 0)
      start = deg[id - 1];
    else
      start = 0;
    if (u % length > 0 && parent[v] == u)
    {
      nextq[start + c] = v;
      c++;
    }
    v = u + 1;
    if (u % length < length - 1 && parent[v] == u)
    {
      nextq[start + c] = v;
      c++;
    }
    v = u - length;
    if (u >= length && parent[v] == u)
    {
      nextq[start + c] = v;
      c++;
    }
    v = u + length;
    if (u + length < size && parent[v] == u)
      nextq[start + c] = v;
  }
}

/* Parallel prefix sum with arbitrary size */
void prefix_sum(std::uint32_t *arr, std::uint32_t s)
{
  if (s <= BLOCK_SIZE)
    in_scan<<<1, BLOCK_SIZE>>>(arr, s, nullptr);
  else
  {
    std::uint32_t aux_s = ceil(s / BLOCK_SIZE);
    std::uint32_t *aux_arr;
    cudaMalloc(&aux_arr, aux_s * sizeof(std::uint32_t));
    in_scan<<<aux_s, BLOCK_SIZE>>>(arr, s, aux_arr);
    prefix_sum(aux_arr, aux_s);
    block_add<<<aux_s, BLOCK_SIZE>>>(arr, s, aux_arr);
    cudaFree(aux_arr);
  }
}
/* Build heatmap with parallel BFS algorithm 
   Both MAP and HEATMAP should be initialize with memory allocated on device */
void heatmap_build(std::uint32_t *map, std::uint32_t *heatmap, std::uint32_t width, std::uint32_t length, std::uint32_t target)
{
  std::uint32_t curq_size, nextq_size, level = 1, size = length * width;
  std::uint32_t *curq, *nextq, *deg, *parent;

  cudaMalloc(&curq, size * sizeof(std::uint32_t));
  cudaMalloc(&nextq, size * sizeof(std::uint32_t));
  cudaMalloc(&deg, size * sizeof(std::uint32_t));

  cudaMalloc(&parent, size * sizeof(std::uint32_t));
  cudaMemset(parent, 0xFF, size * sizeof(std::uint32_t));
  cudaMemset(&parent[target], target, sizeof(std::uint32_t));

  cudaMemcpy(curq, &target, sizeof(std::uint32_t), cudaMemcpyHostToDevice);
  curq_size = 1;
  while (curq_size > 0)
  {
    fill_nextlayer<<<ceil(curq_size / BLOCK_SIZE), BLOCK_SIZE>>>(map, size, curq, curq_size, deg, heatmap, parent, length, level);
    /* Compute the start position each thread would write to avoid data race */
    prefix_sum(deg, curq_size);
    cudaMemcpy(&nextq_size, &deg[curq_size - 1], sizeof(std::uint32_t), cudaMemcpyDeviceToHost);
    assign_nextq<<<ceil(curq_size / BLOCK_SIZE), BLOCK_SIZE>>>(size, curq, curq_size, nextq, deg, parent, length);
    std::uint32_t *temp = curq;
    curq = nextq;
    nextq = temp;
    curq_size = nextq_size;
    level++;
  }
  cudaError_t cudaStatus = cudaGetLastError();
  if (cudaStatus != cudaSuccess) 
  {
    printf("Kernel failed: %s\n", cudaGetErrorString(cudaStatus));
  }

  cudaFree(curq);
  cudaFree(nextq);
  cudaFree(deg);
  cudaFree(parent);
}