using System.Security.Cryptography;
using System.Text;

namespace ModLoadCore.Services;

public class HashService
{
  public async Task<string> ComputeSha1HashAsync(string filePath)
  {
    using SHA1 sha1 = SHA1.Create();
    await using FileStream fileStream = File.OpenRead(filePath);
    var buffer = new byte[8192]; // 可以根据需要调整缓冲区大小
    int bytesRead;
    while ((bytesRead = await fileStream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
    {
      sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
    }
    sha1.TransformFinalBlock(buffer, 0, 0); // 完成最后的哈希计算
    var hashBytes = sha1.Hash!;
    var sb = new StringBuilder();
    for (int i = 0; i < hashBytes.Length; i++)
      sb.Append(hashBytes[i].ToString("x2")); // 使用小写的十六进制字符
    return sb.ToString();
  }
  
  public async Task<uint> ComputeCurseForgeHashAsync(string filePath, uint seed = 1)
  {
    const uint m = 0x5bd1e995;
    const int r = 24;

    // —— 第一遍：只统计 cleanLength ——
    long cleanLength = 0;
    const int bufferSize = 1 << 16; // 64KB 一次读取
    var countBuffer = new byte[bufferSize];

    await using (var fsCount = new FileStream(
                   filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
    {
      int bytesRead;
      while ((bytesRead = await fsCount.ReadAsync(countBuffer, 0, countBuffer.Length)
                                  .ConfigureAwait(false)) > 0)
      {
        for (int i = 0; i < bytesRead; i++)
        {
          byte b = countBuffer[i];
          // 过滤：9(\t)、10(\n)、13(\r)、32(空格)
          if (b != 9 && b != 10 && b != 13 && b != 32)
            cleanLength++;
        }
      }
    }

    if (cleanLength > uint.MaxValue)
      throw new InvalidOperationException("数据过长，cleanLength 超过 uint.MaxValue。");

    // 初始化 h
    uint h = seed ^ (uint)cleanLength;

    // —— 第二遍：真正做 Hash 计算 —— 
    var readBuffer = new byte[bufferSize];
    uint partialK = 0;
    int partialCount = 0; // 当前在 partialK 中已收集了多少个非空白字节（0–3）

    await using (var fsHash = new FileStream(
                   filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
    {
      int bytesRead;
      while ((bytesRead = await fsHash.ReadAsync(readBuffer, 0, readBuffer.Length)
                                  .ConfigureAwait(false)) > 0)
      {
        for (int i = 0; i < bytesRead; i++)
        {
          byte b = readBuffer[i];
          if (b == 9 || b == 10 || b == 13 || b == 32)
            continue;

          // 按 little-endian 累积到 partialK
          partialK |= (uint)b << (partialCount * 8);
          partialCount++;

          if (partialCount == 4)
          {
            // 处理整块 k
            uint k = partialK;
            k *= m;
            k ^= k >> r;
            k *= m;

            h *= m;
            h ^= k;

            // 重置
            partialK = 0;
            partialCount = 0;
          }
        }
      }
    }

    // —— 处理尾部（0–3 字节）——
    switch (partialCount)
    {
      case 3:
        h ^= partialK & 0xFF0000;
        goto case 2;
      case 2:
        h ^= partialK & 0x00FF00;
        goto case 1;
      case 1:
        h ^= partialK & 0x0000FF;
        h *= m;
        break;
    }

    // —— 最终混合 —— 
    h ^= h >> 13;
    h *= m;
    h ^= h >> 15;

    return h;
  }
}
