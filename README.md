# LeanCloud Unity SDK forked from Parse

得益于 Parse 开源了 .NET SDK ，因此以个人名义基于 Parse .NET SDK 重写了 LeanCloud Unity SDK。

版权仍属于 Parse，只是基于 LeanCloud REST API 进行的单向兼容。

## 使用步骤
编译之后，拷贝 bin/Release/LeanCloud.Unity.dll 到 Unity 项目里面的 Assets 文件加下，并且在 Assets 新建一个 `link.xml` 的文本文件，拷贝一下内容：

```
<linker>
<assembly fullname="UnityEngine">
<type fullname="UnityEngine.iOS.NotificationServices" preserve="all"/>
<type fullname="UnityEngine.NotificationServices" preserve="all"/>
<type fullname="UnityEngine.iOS.RemoteNotification" preserve="all"/>
<type fullname="UnityEngine.RemoteNotification" preserve="all"/>
<type fullname="UnityEngine.AndroidJavaClass" preserve="all"/>
<type fullname="UnityEngine.AndroidJavaObject" preserve="all"/>
</assembly>
<assembly fullname="LeanCloud.Unity">
<namespace fullname="LeanCloud" preserve="all"/>
<namespace fullname="LeanCloud.Internal" preserve="all"/>
</assembly>
</linker>
```
