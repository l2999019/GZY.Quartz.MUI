# GZY.Quartz.MUI
轻量级 Quartz 可视化管理 UI 组件库
支持平台：.NET 5.0 / .NET 6.0 / .NET 8.0/ .NET 9.0

|Licence| Build | NuGet | Support |
|--|--|--|--|
|![](https://svg.hamm.cn/badge.svg?key=Licence&value=MIT&color=e0861a)|![](https://svg.hamm.cn/badge.svg?key=.Net5.0&value=passing&color=45b97c)|[![](https://img.shields.io/nuget/dt/GZY.Quartz.MUI)](https://www.nuget.org/packages/GZY.Quartz.MUI)|.Net5.0&.Net6.0&.Net8.0&.Net9.0

## ✨ 功能特性

- 📊 **可视化管理 Quartz 作业**：添加、修改、删除、暂停、恢复任务  
- 🔌 **ClassJob 模式支持**：直接通过类定义并注册任务  
- 🗂️ **存储方式可选**：支持文件存储、数据库存储两种模式  
- 📦 **Razor Class Library (RCL)** 打包：静态资源嵌入程序集，开箱即用  
- 🛠️ **无侵入集成**：通过中间件和服务扩展快速接入
- 🔒 **支持简易授权**：支持Basic简易授权,让界面更安全

---

## 📦 安装

NuGet 安装：  

```bash
dotnet add package GZY.Quartz.MUI
```

## 🚀 快速开始

一、文件存储版本（适合轻量应用)

1.在 Program.cs 或 Startup.cs 注册服务： 

```csharp
services.AddQuartzUI();
services.AddQuartzClassJobs(); // 如果需要 ClassJob 模式
```
2.在 Configure 中启用中间件：
```csharp
app.UseQuartz();
```

二、数据库存储版本(适合中大型需持久化任务场景,以mysql为例)

1.在 Program.cs 或 Startup.cs 注册服务：
```csharp
var optionsBuilder = new DbContextOptionsBuilder<QuarzEFContext>();
optionsBuilder.UseMysql("server=xxxxxxx;database=xxx;User Id=xxxx;PWD=xxxx", b => b.MaxBatchSize(1));//创建数据库连接
services.AddQuartzUI(optionsBuilder.Options); //注入UI组件
```
2.在 Configure 中启用中间件：
```csharp
app.UseQuartz();
```

三、启动应用,并输入地址后缀/QuartzUI,比如:
```csharp
localhost:5260/QuartzUI
```

四、简易Basic授权,框架自带Basic授权可以直接启动,代码如下:
```csharp
 app.UseQuartzUIBasicAuthorized();//注意:要在app.UseQuartz()之前注入授权.
 app.UseQuartz(); 
```
默认账户名密码为:Admin,123456
可以通过在配置文件中添加配置修改.如下:
```json
 "QuartzUI": {
   "UserName": "xxx",
   "Pwd": "xxx"
 },
```
中文详细使用方法请参考:
https://www.cnblogs.com/GuZhenYin/p/15411316.html  

## 📸 运行效果
运行项目即可   
<img width="1883" height="531" alt="微信图片_20250915092354_4272" src="https://github.com/user-attachments/assets/f86ac1e3-66b0-44a7-8cb2-3d6cd13ae7eb" />
<img width="1893" height="950" alt="微信图片_20250915092334_4271" src="https://github.com/user-attachments/assets/b4e9adc7-30ea-49ce-9145-48e40e14fb0f" />

运行效果如下:
  ![653851-20211229145753683-274021795](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/3bcafe20-b779-48ab-a51d-67afcb199601)

---

## 🤝 注意事项
有个比较重要的注意事项
因为组件使用RCL的技术实现的,所以在开发环境需要手动添加一下静态资源包
.NET5.0的兄弟应该在Program类中添加如下代码:
```csharp
webBuilder.UseStaticWebAssets();
```
如图:
![image](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/0c5cd8b7-00e8-439b-8131-58bfd5a1acc0)
.NET6.0+的兄弟 应该添加如下代码:
![image](https://github.com/l2999019/GZY.Quartz.MUI/assets/10385855/cc0034ba-d126-463e-bca7-7bed395d3726)

## 📝 更新说明
 ### 2.8 更新说明:
 注意:2.8如果是数据库存储并从老版本更新的话 请手动添加JobStatus,DurationMs字段</br>
 Mysql例子如下:</br>
 ```sql
  ALTER TABLE `tab_quarz_tasklog` ADD COLUMN `DurationMs` int NOT NULL COMMENT '任务耗时(毫秒)';
  ALTER TABLE `tab_quarz_tasklog` ADD COLUMN `JobStatus` int NOT NULL COMMENT '任务执行结果';
 ```
 1.新增任务界面搜索功能,可根据任务名称和任务分组名进行检索</br>
 2.新增任务仪表盘界面,方便随时监控定时任务情况</br>
 3.修复秒级任务在项目启动时偶尔会执行一次的BUG </br>
 4.修复API类型的任务异常被忽略,无法显示的问题</br>
 5.优化文件存储时,对文件操作的并发控制，确保在多线程环境下的安全性</br>
 6.组件相关支持到.net9.0</br>
 
 ---
 ### 2.7 更新说明:
 注意:2.7如果是数据库存储并从老版本更新的话,请手动给tab_quarz_task表添加ApiTimeOut字段</br>
 Mysql例子如下:</br>
 ```sql
 ALTER TABLE `tab_quarz_task` ADD COLUMN `ApiTimeOut` int NULL;
 ```
 1.添加API类任务的超时时间,可以通过全局配置也可以单个任务设置</br>
 2.设置定时任务日志查看默认按开始时间倒序</br>
 3.添加是否显示控制台日志的全局配置 </br>
   目前支持两个参数:<br />
   `ShowConsoleLog //是否显示控制台日志` <br />
   `DefaultApiTimeOut //默认全局API超时时间` <br />
   初始化时,添加代码如下:
   ```csharp
   builder.Services.AddQuartzUI(quartzMUIOptions: new QuartzMUIOptions() { ShowConsoleLog=false,DefaultApiTimeOut=10});
   ```
 4.优化UI显示-固定操作栏和表头,方便任务较多的情况下操作</br>
 5.优化UI显示-执行记录消息添加支持br关键字进行换行查看</br>
 6.修复API类定时任务在没有参数的情况下会报错的问题</br>
 
 ---


## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=l2999019/GZY.Quartz.MUI&type=Date)](https://www.star-history.com/#l2999019/GZY.Quartz.MUI&Date)

