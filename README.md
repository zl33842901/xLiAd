# xLiAd.Cos

Ϊ ��Ѷ��COS �洢Ͱ��д��SDK

���� COS �洢��ʱ�򣬷���û�� C# �汾�� SDK�����еļ���ǰ汾�Ƚ��Ͼɣ������õġ�

Ȼ����Լ�д�˸���

### ��װ��

dotnet add package xLiAd.Cos

### ʹ�÷���
```csharp
var cos = new Cos(appid, secretId, secretKey, bucketName, region);
cos.Put("/1.jpg", "C:\\1.jpg");
bool b = cos.Exists("/1.jpg");
var bytes = cos.Get("/1.jpg");
cos.Delete("/1.jpg");
```