# xLiAd.Cos

为 腾讯云COS 存储桶编写的SDK

我用 COS 存储的时候，发现没有 C# 版本的 SDK，现有的几款都是版本比较老旧，不能用的。

然后就自己写了个。

### 安装包

dotnet add package xLiAd.Cos

### 使用方法
```csharp
var cos = new Cos(appid, secretId, secretKey, bucketName, region);
cos.Put("/1.jpg", "C:\\1.jpg");
bool b = cos.Exists("/1.jpg");
var bytes = cos.Get("/1.jpg");
cos.Delete("/1.jpg");
```