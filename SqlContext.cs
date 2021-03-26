using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shortlink
{
    public class SqlContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string Db = "Filename="
                + AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings").GetSection("SQLLiteContextDB").Value;
            optionsBuilder.UseSqlite(Db);
        }

        public SqlContext() : base()
        {
        }
        public DbSet<ShortLink> ShortLink { get; set; } //在数据库中生成数据表A

    }

    //在数据库有对应表的叫做实体
    public class ShortLink
    {
        [Key]
        public string Name { get; set; }
        public string OrgLink { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

    }

    //在数据库中没有对应表的叫做模型
    public class Result
    {
        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool IsSucceed { get; set; }
        /// <summary>
        /// 返回类型
        /// </summary>
        public OperationType Type { get; set; }
        public object Content { get; set; }
        public string Message { get; set; }
    }

    public enum RquestType
    {
        url,
        json
    }

    public enum OperationType
    {
        @goto,
        addOne,
        delOne,
        getOne,
        setOne,
        getAll,
        name //url参数
    }
}
