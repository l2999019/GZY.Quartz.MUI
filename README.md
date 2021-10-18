# GZY.Quartz.MUI
基于Quartz的轻量级,注入化的UI组件

中文使用方法请参考:
https://www.cnblogs.com/GuZhenYin/p/15411316.html


简易步骤:
1.注入QuartzUI
var optionsBuilder = new DbContextOptionsBuilder<QuarzEFContext>();
optionsBuilder.UseMysql("server=xxxxxxx;database=xxx;User Id=xxxx;PWD=xxxx", b => b.MaxBatchSize(1));//创建数据库连接
services.AddQuartzUI(optionsBuilder.Options); //注入UI组件

2.在Startup的Configure方法中添加以下内容:
app.UseQuartz();
  
  
运行项目即可 

注:界面参考Quartz.NetUI

