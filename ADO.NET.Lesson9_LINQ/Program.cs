using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADO.NET.Lesson9_LINQ
{
    public class Area
    {
        public int AreaId { get; set; }
        public int TypeArea { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public bool? NoSplit { get; set; }
        public bool? AssemblyArea { get; set; }
        public string FullName { get; set; }
        public bool? MultipleOrders { get; set; }
        public bool? HiddenArea { get; set; }
        public string IP { get; set; }
        public int PavilionId { get; set; }
        public int TypeId { get; set; }
        public int OrderExecution { get; set; }
        public int Dependence { get; set; }
        public int WorkingPeople { get; set; }
        public int ComponentTypeId { get; set; }
        //public Timer timer { get; set; }
    }

    public class Timer
    {
        public int TimerId { get; set; }
        public int UserId { get; set; }
        public int AreaId { get; set; }
        public int DocumentId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateFinish { get; set; }
        public int DurationInSeconds { get; set; }
    }


    class Program
    {
        static SqlConnection connection = new SqlConnection(
            @"Server=VLADIMIR\SQLEXPRESS; Database=CRCMS_AdoLesson9; User Id=ova; Password=123");
        static SqlCommand command = new SqlCommand();
        static SqlDataReader dataReader = null;
        static List<Area> areas = new List<Area>();

        static void Main(string[] args)
        {
            //command.Connection = connection;
            //command.CommandText = "SELCT * FROM Area";

            //using (connection)
            //{
            //    dataReader = command.ExecuteReader();

            //    while (dataReader.Read())
            //    {
            //        Object area = Activator.CreateInstance(typeof(Area));

            //        foreach(PropertyInfo prop in area.GetType().GetProperties())
            //        {
            //            area.GetType()
            //                .GetProperty(prop.Name)
            //                .SetValue(area, dataReader[prop.Name]);
            //        }
            //        areas.Add(area as Area);
            //    }

            //}

            areas = GetData<Area>(command);
            //Example2();
            HomeWorkTasks();

            Console.ReadKey();
        }

        public static List<T> GetData<T>(SqlCommand cmd)
        {
            List<T> collection = new List<T>();

            cmd.Connection = connection;

            // если команда пришедшая в метод пустая
            cmd.CommandText = string.IsNullOrWhiteSpace(cmd.CommandText)
                ? "SELECT * FROM " + typeof(T).Name
                : cmd.CommandText;

            using (connection)
            {
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Object istanance = Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo prop in istanance.GetType().GetProperties())
                    {
                        istanance.GetType()
                            .GetProperty(prop.Name)
                            .SetValue(istanance, rdr[prop.Name] != DBNull.Value
                            ? rdr[prop.Name]
                            : null);

                    }
                    collection.Add((T)Convert.ChangeType(istanance, typeof(T)));
                }
            }
            return collection;
        }

        public static List<T> GetDataTimer<T>(SqlCommand cmd)
        {
            List<T> collection = new List<T>();

            cmd.Connection = connection;
            cmd.CommandText = string.IsNullOrWhiteSpace(cmd.CommandText)
                ? "select * from " + typeof(T).Name
                : cmd.CommandText;

            using (connection)
            {
                connection.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {

                    Object istanance = Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo prop in istanance.GetType().GetProperties())
                    {
                        if(!prop.GetType().IsClass)
                        istanance.GetType()
                            .GetProperty(prop.Name)
                            .SetValue(istanance, rdr[prop.Name] != DBNull.Value
                            ? rdr[prop.Name]
                            : null);

                    }
                    collection.Add((T)Convert.ChangeType(istanance, typeof(T)));
                }
            }
            return collection;
        }


        // Select
        static void Example()
        {
            List<Area> areas = GetData<Area>(new SqlCommand());

            IEnumerable<string> q1 = areas.Select(s => s.FullName + "(" + s.IP + ")");
            
            IEnumerable<object> q2 = areas.Select(s => new 
            { 
               FullName = s.FullName + "(" + s.IP + ")"     
            });

            var q3 = from s in areas
                     select new
                     {
                         FullName = s.FullName + "(" + s.IP + ")"
                     };
        }

        // SelectMany
        static void Example1()
        {
            string[] names = new string[] { "Y G", "K C", "I P", };
            string test = "Vladimir Orlov";
            string[] childS = test.Split();

            // сборка входного элемента в одну последовательность
            IEnumerable<string> query = names.SelectMany(n => n.Split());
        }

        // SelectMany
        static void Example2()
        {
            string[] names = new string[] { "Y", "K", "I", };

            // вывод комбинаций содержимого массива names
            var query = from name1 in names
                        from name2 in names
                        select name1 + " - " + name2;

            var query1 = from name1 in names
                         from name2 in names
                         where name1.CompareTo(name2) < 0  // исключаем соединение друг с другом
                         select name1 + " - " + name2;

            foreach (var item in query1)
            {
                Console.WriteLine(item);
            }
        }


        // Join
        static void Example3()
        {
            List<Area> areas = new List<Area>();
            List<Timer> timers = new List<Timer>();


            var query = from a in areas
                        select new
                        {
                            a.FullName,
                            timers = from t in timers
                                     where t.AreaId == a.AreaId
                                     select t
                        };

            //var query1 = from a in areas
            //             from t in a.timers
            //             where a.AreaId == 1
            //             select new { a.FullName, t.DataStart, t.DateFinish };

        }

        // Join GroupJoin
        static void Example4()
        {
            List<Area> areas = new List<Area>();
            List<Timer> timers = new List<Timer>();

            // Join

            var query = from a in areas
                        join t in timers
                        on a.AreaId equals t.AreaId
                        select new { a.FullName, t.DateStart, t.DateFinish }; 

            var query1 = areas.Join(timers, 
                t=> t.AreaId, a=> a.AreaId,
                (a,t)=> new { a.FullName, t.DateStart, t.DateFinish });
        }

        // OrderBy 
        // ThenBy
        // OrderByDescending
        // Revers
        static void Example5()
        {
            List<Area> areas = new List<Area>();

            var query = areas.OrderBy(o => o.FullName);
            var query1 = areas.OrderByDescending(o => o.FullName);
            var query2 = areas.OrderByDescending(o => o.FullName) // сортировка по имени
                              .ThenBy(t => t.WorkingPeople);      // сортировка по людям

            //List<Timer> timers = new List<Timer>(new SqlCommand());


        }

        //============================================================================================
        /// PractWOrk ///
        /// 

        static void HomeWorkTasks()
        {
            /*5.	Реализовать справочник, который возвращает ID зоны/участка, 
             * и IP адрес данной зоны/участка. Так же необходимо исключить 
             * зоны/участки у которых не заполнено поле IP*/

            var task5 = areas.Where(w => !string.IsNullOrEmpty(w.IP))
                             .Select(s => new { s.AreaId, s.IP });
            //.OrderByDescending(o=> o.AreaId);

            /*
            foreach (var item in task5)
            {
                Console.WriteLine(item.AreaId + " --- " + item.IP);
            }
            */

            /*6.	Реализовать справочник, который возвращает IP адрес и касс Area,
             * исключить все зоны/участки, у которых отсутствует IPадрес, а так же 
             * исключить все дочерние зоны/участки (ParentId!=0)*/

            var task6 = areas.Where(w => !string.IsNullOrEmpty(w.IP) && w.ParentId != 0)
                             .Select(s => new { IP = s.IP, Obj = s });

            /*
            foreach (var item in task6)
            {
                Console.WriteLine(item.IP + " --- " + item.Obj.Name);
            }
            */

            /*7.	Используя коллекцию Lookup, вернуть следующие данные. В качестве
             * ключа использовать IP адрес, в качестве значения использовать класс Area*/

            ILookup<string, Area> task7 = areas.ToLookup(t => t.IP, t => t);

            /*8.	Вернуть первую запись из последовательности, где HiddenArea=1*/

            var task8 = areas.Where(w => w.HiddenArea == true).FirstOrDefault();
            Console.WriteLine(task8.AreaId);

            /*9.	Вернуть последнюю запись из таблицы Area, указав следующий 
             * фильтр – PavilionId = 1*/

            var task9 = areas.Where(w => w.PavilionId == 1).LastOrDefault();
            Console.WriteLine(task9.AreaId);

            /*10.	Используя квантификаторы, вывесит на экран значения следующих фильтров:
                    a.	Есть ли в таблице зоны/участки для PavilionId = 1 и 
                        IP = 10.53.34.85, 10.53.34.77, 10.53.34.53
                        */

            var task10a = areas.Any(a => a.PavilionId == 1 &&
                                   (a.IP == "10.53.34.85" || a.IP == "10.53.34.77" || a.IP == "10.53.34.53"));

            Console.WriteLine(task10a ? "Содержится" : "Не содержится");
            /*
                    b.	Содержатся ли данные в таблице Area с наименованием зон/участков
                        - PT disassembly, Engine testing
            */

            var task10b = areas.Any(a => a.Name.Contains("PT disassembly") || a.Name.Contains("Engine testing"));
            Console.WriteLine(task10b? "Содержится" : "Не содержится");

            /*11.	Вывести сумму всех работающих работников (WorkingPeople) на зонах*/

            var task11 = areas.Sum(s => s.WorkingPeople);
            Console.WriteLine($"сумма всех работающих работников (WorkingPeople)" + task11); 

        }

    }
}
