# Resolved: https://github.com/dotnet/runtime/issues/104001

# ConditionalSelectReproduce

Reproduce for the issue of JIT optimization for the `VectorT.ConditionalSelect` (particularly 128-bit) in comparison with `Sse41.BlendVariable`.

### Benchmark results

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3737/23H2/2023Update/SunValley3)
AMD Ryzen 7 5800X3D, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.100-preview.5.24307.3
  [Host]     : .NET 9.0.0 (9.0.24.30607), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.30607), X64 RyuJIT AVX2


```
| Method                 | Mean      | Error     | StdDev    | Code Size |
|----------------------- |----------:|----------:|----------:|----------:|
| ConditionalSelectField | 0.3298 ns | 0.0038 ns | 0.0034 ns |      54 B |
| BlendVariable          | 0.1897 ns | 0.0067 ns | 0.0059 ns |      52 B |

## Disassembly

### Disassembly diff

```diff
## .NET 9.0.0 (9.0.24.30607), X64 RyuJIT AVX2
```assembly
-; ConditionalSelectBenchmark.BlendVariable()
+; ConditionalSelectBenchmark.ConditionalSelectField()
       sub       rsp,28
       mov       rax,vectorOneAddr
       mov       rax,[rax]
-      vmovups   xmm0,[base]
+      vmovups   xmm0,[rax+10]
-      vxorps    xmm1,xmm1,xmm1
+      vpand     xmm1,xmm0,[7FFE613076D0]
+      vxorps    xmm2,xmm2,xmm2
-      vmovups   xmm2,[rax+10]
-      vpblendvb xmm0,xmm0,xmm1,xmm2
+      vpandn    xmm0,xmm0,xmm2
+      vpor      xmm0,xmm0,xmm1
       vmovd     eax,xmm0
       movzx     eax,al
       add       rsp,28
       ret
- ; Total bytes of code 52
+ ; Total bytes of code 54
```

So, we've got `vpand` + `vpandn` + `vpor` instead of `vmovups` (not sure why we need it anyway, probably can be optimized?) + `vpblendvb`.

<details>
  <summary>Full disassembly</summary>

  ### .NET 9.0.0 (9.0.24.30607), X64 RyuJIT AVX2
  ```assembly
  ; ConditionalSelectBenchmark.ConditionalSelectField()
         sub       rsp,28
         mov       rax,21971404218
         mov       rax,[rax]
         vmovups   xmm0,[rax+10]
         vpand     xmm1,xmm0,[7FFE613076D0]
         vxorps    xmm2,xmm2,xmm2
         vpandn    xmm0,xmm0,xmm2
         vpor      xmm0,xmm0,xmm1
         vmovd     eax,xmm0
         movzx     eax,al
         add       rsp,28
         ret
  ; Total bytes of code 54
  ```

  ### .NET 9.0.0 (9.0.24.30607), X64 RyuJIT AVX2
  ```assembly
  ; ConditionalSelectBenchmark.BlendVariable()
         sub       rsp,28
         mov       rax,1D49BC04218
         mov       rax,[rax]
         vmovups   xmm0,[7FFE612E76C0]
         vxorps    xmm1,xmm1,xmm1
         vmovups   xmm2,[rax+10]
         vpblendvb xmm0,xmm0,xmm1,xmm2
         vmovd     eax,xmm0
         movzx     eax,al
         add       rsp,28
         ret
  ; Total bytes of code 52
  ```
</details>
