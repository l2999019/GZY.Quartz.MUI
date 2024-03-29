# GZY.Quartz.MUI
基于Quartz的轻量级,注入化的UI组件

|Licence| Build | NuGet | Support |
|--|--|--|--|
|![](https://svg.hamm.cn/badge.svg?key=Licence&value=MIT&color=e0861a)|![](https://svg.hamm.cn/badge.svg?key=.Net5.0&value=passing&color=45b97c)|[![](https://img.shields.io/nuget/dt/GZY.Quartz.MUI)](https://www.nuget.org/packages/GZY.Quartz.MUI)|.Net5.0&.Net6.0&.Net8.0


中文使用方法请参考:
https://www.cnblogs.com/GuZhenYin/p/15411316.html
简易步骤: 
本地文件存储版本:  
1.注入QuartzUI  
  services.AddQuartzUI();  
2.如需开启ClassJob则注入以下内容  
  services.AddQuartzClassJobs();  
  
数据库版本 
1.注入QuartzUI  
var optionsBuilder = new DbContextOptionsBuilder<QuarzEFContext>();  
optionsBuilder.UseMysql("server=xxxxxxx;database=xxx;User Id=xxxx;PWD=xxxx", b => b.MaxBatchSize(1));//创建数据库连接  
services.AddQuartzUI(optionsBuilder.Options); //注入UI组件  

2.在Startup的Configure方法中添加以下内容:  
app.UseQuartz();  
  
  
运行项目即可   
![image](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/65ca3bdc-587e-486d-ab9b-e3502d361fd2)
运行效果如下:
  ![653851-20211229145753683-274021795](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/3bcafe20-b779-48ab-a51d-67afcb199601)

有个比较重要的注意事项,一直忘记说了,这里也提一下

因为组件使用RCL的技术实现的,所以在开发环境需要手动添加一下静态资源包

.NET5.0的兄弟应该在Program类中添加如下代码:

webBuilder.UseStaticWebAssets();

如图:
![image](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/0c5cd8b7-00e8-439b-8131-58bfd5a1acc0)

 

.NET6.0+的兄弟 应该添加如下代码:
![image](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/cc0034ba-d126-463e-bca7-7bed395d3726)

   
  
  
注:界面参考Quartz.NetUI

