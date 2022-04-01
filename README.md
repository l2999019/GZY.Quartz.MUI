# GZY.Quartz.MUI
基于Quartz的轻量级,注入化的UI组件

|Licence| Build | NuGet | Support |
|--|--|--|--|
|![](https://svg.hamm.cn/badge.svg?key=Licence&value=MIT&color=e0861a)|![](https://svg.hamm.cn/badge.svg?key=.Netcore3.1&value=passing&color=45b97c)|[![](https://img.shields.io/nuget/dt/GZY.Quartz.MUI)](https://www.nuget.org/packages/GZY.Quartz.MUI)|![](https://svg.hamm.cn/badge.svg?key=.NetCore3.1 .Net5.0 .Net5.0&value=Support&color=00FF00)


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
  
    
   
  
  
注:界面参考Quartz.NetUI

