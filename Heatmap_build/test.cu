#include "heatmap.cuh"
#define BOUND 40

void gen_map(std::uint32_t *map, std::uint32_t size)
{
  for (std::uint32_t i = 0; i < size; i++)
  {
    if (rand() % 100 > BOUND)
      map[i] = UNREACHABLE;
    else
      map[i] = REACHABLE;
  }
}

void test_prefix_sum()
{
  std::uint32_t s = 768968, res;
  std::uint32_t *arr, *one_for_all;
  cudaMalloc(&arr, s * sizeof(std::uint32_t));
  one_for_all = new std::uint32_t[s];
  for (std::uint32_t i = 0; i < s; i++)
    one_for_all[i] = 1;
  cudaMemcpy(arr, one_for_all, s * sizeof(std::uint32_t), cudaMemcpyHostToDevice);
  prefix_sum(arr, s);
  cudaMemcpy(&res, &arr[s - 1], sizeof(std::uint32_t), cudaMemcpyDeviceToHost);
  printf("%d\n", res);
  delete[] one_for_all;
  cudaFree(arr);
}

void print_heatmap(std::uint32_t *heatmap, std::uint32_t length, std::uint32_t width)
{
  for (std::uint32_t i = 0; i < width; i++)
  {
    for (std::uint32_t j = 0; j < length; j++)
    {
      printf("%3d ", heatmap[i * length + j]);
    }
    printf("\n");
  }
}

void init_heatmap(std::uint32_t *heatmap, std::uint32_t size, std::uint32_t target)
{
  memset(heatmap, 0xFF, size * sizeof(std::uint32_t));
  heatmap[target] = 0;
}

void test_heatmap()
{
  std::uint32_t width = 1 << 5, length = 1 << 5, size = width * length, target = rand() % size;
  printf("Target: %d row %d line\n", target / length, target % length);
  std::uint32_t *d_map, *map, *d_heatmap, *heatmap;
  cudaMalloc(&d_map, size * sizeof(std::uint32_t));
  cudaMalloc(&d_heatmap, size * sizeof(std::uint32_t));
  map = new std::uint32_t[size];
  heatmap = new std::uint32_t[size];
  gen_map(map, size);
  init_heatmap(heatmap, size, target);
  cudaMemcpy(d_map, map, size * sizeof(std::uint32_t), cudaMemcpyHostToDevice);
  cudaMemcpy(d_heatmap, heatmap, size * sizeof(std::uint32_t), cudaMemcpyHostToDevice);
  heatmap_build(d_map, d_heatmap, width, length, target);
  cudaMemcpy(heatmap, d_heatmap, size * sizeof(std::uint32_t), cudaMemcpyDeviceToHost);
  print_heatmap(map, length, width);
  printf("\n");
  print_heatmap(heatmap, length, width);
  delete[] map;
  delete[] heatmap;
  cudaFree(d_map);
  cudaFree(d_heatmap);
}

int main()
{
  //test_prefix_sum();
  test_heatmap();
}