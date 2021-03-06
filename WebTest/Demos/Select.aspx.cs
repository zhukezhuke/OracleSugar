﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Models;
using WebTest.Dao;
using OracleSugar;
using System.Data;
namespace WebTest.Demo
{
    /// <summary>
    /// 查询例子
    /// </summary>
    public partial class Select : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //使用拉姆达查询 基于Queryable
            QueryableDemo();

            //使用更接近sql的查询方式 基于Sqlable
            SqlableDemo();

            //使用原生Sql查询 
            SqlQuery();

            //新容器转换
            SelectNewClass();

        }

        /// <summary>
        /// 新容器转换
        /// </summary>
        private void SelectNewClass()
        {

            using (SqlSugarClient db = SugarDao.GetInstance())
            {
                var list2 = db.Queryable<STUDENT>().Where(c => c.ID < 10).Select(c => new classNew { NEWID = c.ID, NEWNAME = c.NAME, XX_NAME = c.NAME }).ToList();//不支持匿名类转换,也不建议使用

                var list3 = db.Queryable<STUDENT>().Where(c => c.ID < 10).Select(c => new { newid = c.ID, newname = c.NAME, xx_name = c.NAME }).ToDynamic();//匿名类转换

                var list4 = db.Queryable<STUDENT>().Where(c => c.ID < 10).Select("id as newid, name as newname ,name as xx_name").ToDynamic();//匿名类转换

                var jList1 = db.Queryable<STUDENT>()
                 .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) // left join  School s2  on s1.id=s2.id
                 .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID > 1)  // where s1.id>1
                 .OrderBy<STUDENT, SCHOOL>((s1, s2) => s1.ID) //order by s1.id 多个order可以  .oderBy().orderby 叠加 
                 .Skip(1)
                 .Take(2)
                 .Select<STUDENT, SCHOOL, classNew>((s1, s2) => new classNew() { NEWID = s1.ID, NEWNAME = s2.NAME, XX_NAME = s1.NAME })//select目前只支持这种写法
                 .ToList();

                var jList2 = db.Queryable<STUDENT>()
                .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) // left join  School s2  on s1.id=s2.id
                .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID > 1)  // where s1.id>1
                .OrderBy<STUDENT, SCHOOL>((s1, s2) => s1.ID) //order by s1.id 多个order可以  .oderBy().orderby 叠加 
                .Skip(1)
                .Take(2)
                .Select<STUDENT, SCHOOL, classNew>((s1, s2) => new classNew() { NEWID = s1.ID, NEWNAME = s1.NAME, XX_NAME = s1.NAME })//select目前只支持这种写法
                .ToDynamic();


                var jList3 = db.Queryable<STUDENT>()
                .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) // left join  School s2  on s1.id=s2.id
                .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID > 1)  // where s1.id>1
                .OrderBy<STUDENT, SCHOOL>((s1, s2) => s1.ID) //order by s1.id 多个order可以  .oderBy().orderby 叠加 
                .Skip(1)
                .Take(2)
                .Select<STUDENT, classNew>(s1 => new classNew() { NEWID = s1.ID, NEWNAME = s1.NAME, XX_NAME = s1.NAME })//select目前只支持这种写法
                .ToDynamic();
            }
        }

        /// <summary>
        /// 基于原生Sql的查询
        /// </summary>
        private void SqlQuery()
        {
            using (var db = SugarDao.GetInstance())
            {
                //转成list
                List<STUDENT> list1 = db.SqlQuery<STUDENT>("select * from Student");
                //转成list带参
                List<STUDENT> list2 = db.SqlQuery<STUDENT>("select * from Student where id=:id", new { id = 1 });
                //转成dynamic
                dynamic list3 = db.SqlQueryDynamic("select * from student");
                //转成json
                string list4 = db.SqlQueryJson("select * from student");
                //返回int
                var list5 = db.SqlQuery<int>("select   id from Student where rownum=1").SingleOrDefault();
                //反回键值
                Dictionary<string, string> list6 = db.SqlQuery<KeyValuePair<string, string>>("select id,name from Student").ToDictionary(it => it.Key, it => it.Value);
                //反回List<string[]>
                var list7 = db.SqlQuery<string[]>("select   id,name from Student where rownum=1").SingleOrDefault();

                //存储过程
                //var spResult = db.SqlQuery<SCHOOL>(@"begin exec sp_school (:p1,:p2); end;", new { p1 = 1, p2 = 2 });

                //获取第一行第一列的值
                string v1 = db.GetString("select '张三' as name from dual");
                int v2 = db.GetInt("select 1 as name  from dual");
                double v3 = db.GetDouble("select 1 as name  from dual");
                decimal v4 = db.GetDecimal("select 1 as name  from dual");
                //....
            }
        }
        /// <summary>
        /// 接近Sql的编程模式
        /// </summary>
        private void SqlableDemo()
        {
            using (var db = SugarDao.GetInstance())
            {
                //---------Sqlable,创建多表查询---------//

                //多表查询
                List<SCHOOL> dataList = db.Sqlable()
                   .From("school", "s")
                   .Join("student", "st", "st.id", "s.id", JoinType.INNER)
                   .Join("student", "st2", "st2.id", "st.id", JoinType.LEFT)
                   .Where("s.id>100 and s.id<:id")
                   .Where("1=1")//可以多个WHERE
                   .OrderBy("s.id")
                   .SelectToList<SCHOOL/*新的Model我这里没有所以写的School*/>("st.*", new { id = 1 });

                //多表分页
                List<SCHOOL> dataPageList = db.Sqlable()
                    .From("school", "s")
                    .Join("student", "st", "st.id", "s.id", JoinType.INNER)
                    .Join("student", "st2", "st2.id", "st.id", JoinType.LEFT)
                    .Where("s.id>100 and s.id<100")
                    .SelectToPageList<SCHOOL>("st.*", "s.id", 1, 10);

                //多表分页WHERE加子查询
                List<SCHOOL> dataPageList2 = db.Sqlable()
                    .From("school", "s")
                    .Join("student", "st", "st.id", "s.id", JoinType.INNER)
                    .Join("student", "st2", "st2.id", "st.id", JoinType.LEFT)
                    .Where("s.id>100 and s.id<100 and s.id in (select 1 as id from dual )" /*这里面写子查询都可以*/)
                    .SelectToPageList<SCHOOL>("st.*", "s.id", 1, 10);



                //--------转成List Dynmaic 或者 Json-----//

                //不分页
                var list1 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToDynamic("*", new { id = 1 });
                var list2 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToJson("*", new { id = 1 });
                var list3 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToDataTable("*", new { id = 1 });

                //分页
                var list4 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToPageDynamic("s.*", "l.id", 1, 10, new { id = 1 });
                var list5 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToPageTable("s.*", "l.id", 1, 10, new { id = 1 });
                var list6 = db.Sqlable().From("student", "s").Join("school", "l", "s.sch_id", "l.id and l.id=:id", JoinType.INNER).SelectToPageDynamic("s.*", "l.id", 1, 10, new { id = 1 });


                //--------拼接-----//
                Sqlable sable = db.Sqlable().From<STUDENT>("s").Join<SCHOOL>("l", "s.sch_id", "l.id", JoinType.INNER);
                string name = "a";
                int id = 1;
                if (!string.IsNullOrEmpty(name))
                {
                    sable = sable.Where("s.name=:name");
                }
                if (!string.IsNullOrEmpty(name))
                {
                    sable = sable.Where("s.id=:id or s.id=100");
                }
                if (id > 0)
                {
                    sable = sable.Where("l.id in (select  id from school where rownum<10)");//where加子查询
                }
                var pars = new { id = id, name = name };
                int pageCount = sable.Count(pars);
                var list7 = sable.SelectToPageList<STUDENT>("s.*", "l.id desc", 1, 20, pars);


            }
        }

        /// <summary>
        /// 拉姆达表达示
        /// </summary>
        private void QueryableDemo()
        {

            using (var db = SugarDao.GetInstance())
            {


                //---------Queryable<T>,扩展函数查询---------//

                //针对单表或者视图查询

                //查询所有
                var student = db.Queryable<STUDENT>().ToList();
                var studentDynamic = db.Queryable<STUDENT>().ToDynamic();
                var studentJson = db.Queryable<STUDENT>().ToJson();

                //查询单条
                var single = db.Queryable<STUDENT>().Single(c => c.ID == 1);
                //查询单条没有记录返回空对象
                var singleOrDefault = db.Queryable<STUDENT>().SingleOrDefault(c => c.ID == 11111111);
                //查询单条没有记录返回空对象
                var single2 = db.Queryable<STUDENT>().Where(c => c.ID == 1).SingleOrDefault();

                //查询第一条
                var first = db.Queryable<STUDENT>().Where(c => c.ID == 1).First();
                var first2 = db.Queryable<STUDENT>().Where(c => c.ID == 1).FirstOrDefault();

                //取10-20条
                var page1 = db.Queryable<STUDENT>().Where(c => c.ID > 10).OrderBy(it => it.ID).Skip(10).Take(20).ToList();

                //上一句的简化写法，同样取10-20条
                var page2 = db.Queryable<STUDENT>().Where(c => c.ID > 10).OrderBy(it => it.ID).ToPageList(2, 10);

                //查询条数
                var count = db.Queryable<STUDENT>().Where(c => c.ID > 10).Count();

                //从第2条开始以后取所有
                var skip = db.Queryable<STUDENT>().Where(c => c.ID > 10).OrderBy(it => it.ID).Skip(2).ToList();

                //取前2条
                var take = db.Queryable<STUDENT>().Where(c => c.ID > 10).OrderBy(it => it.ID).Take(2).ToList();

                // Not like 
                string conval = "a";
                var notLike = db.Queryable<STUDENT>().Where(c => !c.NAME.Contains(conval.ToString())).ToList();
                //Like
                conval = "三";
                var like = db.Queryable<STUDENT>().Where(c => c.NAME.Contains(conval)).ToList();

                //支持字符串Where 让你解决，更复杂的查询
                var student12 = db.Queryable<STUDENT>().Where(c => "a" == "a").Where("id>:id", new { id = 1 }).ToList();
                var student13 = db.Queryable<STUDENT>().Where(c => "a" == "a").Where("id>100 ").ToList();


                //存在记录反回true，则否返回false
                bool isAny100 = db.Queryable<STUDENT>().Any(c => c.ID == 100);
                bool isAny1 = db.Queryable<STUDENT>().Any(c => c.ID == 1);


                //获取最大Id
                object maxId = db.Queryable<STUDENT>().Max(it => it.ID);
                int maxId1 = db.Queryable<STUDENT>().Max(it => it.ID).ObjToInt();//拉姆达
                int maxId2 = db.Queryable<STUDENT>().Max<STUDENT, int>("id"); //字符串写法

                //获取最小
                int minId1 = db.Queryable<STUDENT>().Where(c => c.ID > 0).Min(it => it.ID).ObjToInt();//拉姆达
                int minId2 = db.Queryable<STUDENT>().Where(c => c.ID > 0).Min<STUDENT, int>("id");//字符串写法


                //order By 
                var orderList = db.Queryable<STUDENT>().OrderBy("id desc,name asc").ToList();//字符串支持多个排序
                //可以多个order by表达示
                var order2List = db.Queryable<STUDENT>().OrderBy(it => it.NAME).OrderBy(it => it.ID, OrderByType.desc).ToList(); // order by name as ,order by id desc

                //In
                var intArray = new[] { "5", "2", "3" };
                var intList = intArray.ToList();
                var list0 = db.Queryable<STUDENT>().In(it => it.ID, 1, 2, 3).ToList();
                var list1 = db.Queryable<STUDENT>().In(it => it.ID, intArray).ToList();
                var list2 = db.Queryable<STUDENT>().In("id", intArray).ToList();
                var list3 = db.Queryable<STUDENT>().In(it => it.ID, intList).ToList();
                var list4 = db.Queryable<STUDENT>().In("id", intList).ToList();

                //分组查询
                var list7 = db.Queryable<STUDENT>().Where(c => c.ID < 20).GroupBy(it => it.SEX).Select("sex,count(*) count").ToDynamic();
                var list8 = db.Queryable<STUDENT>().Where(c => c.ID < 20).GroupBy(it => it.SEX).GroupBy(it => it.ID).Select("id,sex,count(*) count").ToDynamic();
                List<SexTotal> list9 = db.Queryable<STUDENT>().Where(c => c.ID < 20).GroupBy(it => it.SEX).Select<STUDENT, SexTotal>("SEX,count(*) COUNT").ToList();
                List<SexTotal> list10 = db.Queryable<STUDENT>().Where(c => c.ID < 20).GroupBy("sex").Select<STUDENT, SexTotal>("SEX,count(*) COUNT").ToList();
                //SELECT Sex,Count=count(*)  FROM Student  WHERE 1=1  AND  (id < 20)    GROUP BY Sex --生成结果



                //2表关联查询
                var jList = db.Queryable<STUDENT>()
                .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) //默认left join
                .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID == 1)
                .Select("s1.*,s2.name as schName")
                .ToDynamic();

                /*等于同于
                 SELECT s1.*,s2.name as schName 
                 FROM [Student]  s1 
                 LEFT JOIN [School]  s2 ON  s1.sch_id  = s2.id 
                 WHERE  s1.id  = 1 */

                //2表关联查询并分页
                var jList2 = db.Queryable<STUDENT>()
                .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) //默认left join
                    //如果要用inner join这么写
                    //.JoinTable<Student, School>((s1, s2) => s1.sch_id == s2.id  ,JoinType.INNER)
                .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID > 1)
                .OrderBy<STUDENT, SCHOOL>((s1, s2) => s1.NAME)
                .Skip(10)
                .Take(20)
                .Select("s1.*,s2.name as schName")
                .ToDynamic();

                //3表查询并分页
                var jList3 = db.Queryable<STUDENT>()
               .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) // left join  School s2  on s1.id=s2.id
               .JoinTable<STUDENT, SCHOOL>((s1, s3) => s1.SCH_ID == s3.ID) // left join  School s3  on s1.id=s3.id
               .Where<STUDENT, SCHOOL>((s1, s2) => s1.ID > 1)  // where s1.id>1
               .Where<STUDENT>(s1 => s1.ID > 0)
               .OrderBy<STUDENT, SCHOOL>((s1, s2) => s1.ID) //order by s1.id 多个order可以  .oderBy().orderby 叠加 
               .Skip(10)
               .Take(20)
               .Select("s1.*,s2.name as schName,s3.name as schName2")//select目前只支持这种写法
               .ToDynamic();


                //上面的方式都是与第一张表join，第三张表想与第二张表join写法如下
                List<classNew> jList4 = db.Queryable<STUDENT>()
                 .JoinTable<STUDENT, SCHOOL>((s1, s2) => s1.SCH_ID == s2.ID) // left join  School s2  on s1.id=s2.id
                 .JoinTable<STUDENT, SCHOOL, AREA>((s1, s2, a1) => a1.ID == s2.AREAID)// left join  Area a1  on a1.id=s2.AreaId
                 .Select<STUDENT, SCHOOL, AREA, classNew>((s1, s2, a1) => new classNew { NEWID = s1.ID, STUDENTNAME = s1.NAME, SCHOOLNAME = s2.NAME, AREANAME = a1.NAME }).ToList();




                //最多支持5表查询,太过复杂的建议用Sqlable或者SqlQuery,我们的Queryable只适合轻量级的查询





                //拼接
                var queryable = db.Queryable<STUDENT>().Where(it => true);
                if (maxId.ObjToInt() == 1)
                {
                    queryable.Where(it => it.ID == 1);
                }
                else
                {
                    queryable.Where(it => it.ID == 2);
                }
                var listJoin = queryable.ToList();


                //queryable和SqlSugarClient解耦
                var par = new Queryable<STUDENT>().Where(it => it.ID == 1);//声名没有connection对象的Queryable
                par.DB = db;
                var listPar = par.ToList();


                //查看生成的sql和参数
                var id=1;
                var sqlAndPars = db.Queryable<STUDENT>().Where(it => it.ID == id).OrderBy(it => it.ID).ToSql();



                //函数的支持(字段暂不支持函数,只有参数支持) 目前只支持这么多
                var par1 = "2015-1-1"; var par2 = "   我 有空格, ";
                var r1 = db.Queryable<STUDENT>().Where(it => it.NAME == par1.ObjToString()).ToSql(); //ObjToString会将null转转成""
                var r2 = db.Queryable<INSERTTEST>().Where(it => it.D1 == par1.ObjToDate()).ToSql();
                var r3 = db.Queryable<INSERTTEST>().Where(it => it.ID == 1.ObjToInt()).ToSql();//ObjToInt会将null转转成0
                var r4 = db.Queryable<INSERTTEST>().Where(it => it.ID == 2.ObjToDecimal()).ToSql();
                var r5 = db.Queryable<INSERTTEST>().Where(it => it.ID == 3.ObjToMoney()).ToSql();
                var r6 = db.Queryable<INSERTTEST>().Where(it => it.V1 == par2.Trim()).ToSql();
                var convert1 = db.Queryable<STUDENT>().Where(c => c.NAME == "a".ToString()).ToList();
                var convert2 = db.Queryable<STUDENT>().Where(c => c.ID == Convert.ToInt32("1")).ToList();// 
                var convert3 = db.Queryable<STUDENT>().Where(c => DateTime.Now > Convert.ToDateTime("2015-1-1")).ToList();
                var convert4 = db.Queryable<STUDENT>().Where(c => DateTime.Now > DateTime.Now).ToList();
                var c1 = db.Queryable<STUDENT>().Where(c =>c.NAME.Contains("a")).ToList();
                var c2 = db.Queryable<STUDENT>().Where(c => c.NAME.StartsWith("a")).ToList();
                var c3 = db.Queryable<STUDENT>().Where(c => c.NAME.EndsWith("a")).ToList();
            }
        }
    }
}