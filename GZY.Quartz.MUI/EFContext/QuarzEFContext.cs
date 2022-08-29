using GZY.Quartz.MUI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZY.Quartz.MUI.EFContext
{
    public class QuarzEFContext : DbContext

    {
        public QuarzEFContext(DbContextOptions<QuarzEFContext> option)
          : base(option)

        {
            var databaseCreator = this.GetService<IRelationalDatabaseCreator>();
           
            if (!databaseCreator.HasTables())
            {
                databaseCreator.EnsureCreated();
            }
            //初始化的时候创建数据库
            //this.Database.EnsureCreated();
            //判断是否有待迁移
            //if (this.Database.GetPendingMigrations().Any())
            //{
            //    Console.WriteLine("检测到实体有改动,正在创建迁移...");
            //    //执行迁移
            //    this.Database.Migrate();
            //    Console.WriteLine("迁移完成");
            //}
            //this.GetService<ILoggerFactory>().AddProvider(new MyFilteredLoggerProvider());
            //options
        }

        #region 添加操作时间
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AddTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseModel && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseModel)entity.Entity).timeflag = DateTime.Now;
                    ((BaseModel)entity.Entity).changetime = DateTime.Now;
                }

                ((BaseModel)entity.Entity).changetime = DateTime.Now;
            }
        }
        #endregion
        /// <summary>
        /// 配置加载
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        /// <summary>
        /// 实体创建
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ddd = modelBuilder.Model.GetEntityTypes().ToList();
            foreach (var item in ddd)
            {
                var tabtype = Type.GetType(item.ClrType.FullName);
                var props = tabtype.GetProperties();
                var descriptionAttrtable = tabtype.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (descriptionAttrtable.Length > 0)
                {
                    modelBuilder.Entity(item.Name).HasComment(((DescriptionAttribute)descriptionAttrtable[0]).Description);
                }
                foreach (var prop in props)
                {
                    var descriptionAttr = prop.GetCustomAttributes(typeof(DescriptionAttribute), true);
                    if (descriptionAttr.Length > 0)
                    {
                        modelBuilder.Entity(item.Name).Property(prop.Name).HasComment(((DescriptionAttribute)descriptionAttr[0]).Description);
                    }
                }
            }

        }

        public DbSet<tab_quarz_task> tab_quarz_task { get; set; }
        public DbSet<tab_quarz_tasklog> tab_quarz_tasklog { get; set; }

    }
}
