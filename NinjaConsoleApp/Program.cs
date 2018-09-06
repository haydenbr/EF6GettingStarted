using NinjaDomain.Classes;
using NinjaDomain.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NinjaConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new NullDatabaseInitializer<NinjaContext>());
            // InsertNinja();
            // SimpleNinjaQueries();
            // QueryAndUpdateNinja();
            // QueryAndUpdateNinjaDisconnected();
            // RetrieveDataWithFind();
            // GetNinjasWithStoredProc();
            // DeleteNinja();
            // DeleteNinjaDisconnected();
            // DeleteNinjaWithStoredProc();
            // InsertNinjaWithEquipment();
            // SimpleNinjaGraphQueryEagerLoading();
            // SimpleNinjaGraphQueryExplicitLoading();
            // SimpleNinjaGraphQueryLazyLoading();
            NinjaProjectionQuery();
        }

        private static void InsertNinja()
        {
            var ninja = new Ninja
            {
                Name = "Michaelangelo",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1985, 1, 1),
                ClanId = 1
            };
            var anotherNinja = new Ninja
            {
                Name = "Leonardo",
                ServedInOniwaban = false,
                DateOfBirth = new DateTime(1984, 1, 1),
                ClanId = 1
            };

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.AddRange(new List<Ninja> { ninja, anotherNinja });
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaQueries()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas
                    .OrderByDescending(n => n.DateOfBirth)
                    .Where(n => n.DateOfBirth >= new DateTime(1970, 1, 1))
                    .Skip(1)
                    .Take(1)
                    .Select(n => new { n.Name, n.Id })
                    .FirstOrDefault();
                if (ninja == null)
                {
                    Console.WriteLine("Oof, couldn't find that ninja");
                }
                else
                {
                    Console.WriteLine("I found this ninja {0}", ninja.Name);
                }
            }
        }

        private static void QueryAndUpdateNinja()
        {
            using(var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.FirstOrDefault();
                ninja.ServedInOniwaban = !ninja.ServedInOniwaban;
                context.SaveChanges();
            }
        }

        private static void QueryAndUpdateNinjaDisconnected()
        {
            Ninja ninja;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }

            ninja.ServedInOniwaban = !ninja.ServedInOniwaban;

            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.Add(ninja);
                context.SaveChanges();
            }
        }

        private static void RetrieveDataWithFind()
        {
            var keyval = 4;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.Find(keyval);
                Console.WriteLine("After Find#1: {0}", ninja.Name);

                var someNinja = context.Ninjas.Find(keyval);
                Console.WriteLine("After Find#2: {0}", someNinja.Name);
                ninja = null;
            }
        }

        private static void GetNinjasWithStoredProc()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninjas = context.Ninjas.SqlQuery("exec dbo.GetOldNinjas").ToList();
                // .ToList here forces query to complete, getting back the results and closign the connection
                // without the .ToList, the connection remains open throughout the for loop below
                foreach (var ninja in ninjas)
                {
                    Console.WriteLine(ninja.Name);
                }
            }
        }

        private static void DeleteNinja()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninja = context.Ninjas.FirstOrDefault();
                context.Ninjas.Remove(ninja);
                context.SaveChanges();
            }
        }

        private static void DeleteNinjaDisconnected()
        {
            Ninja ninja;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                ninja = context.Ninjas.FirstOrDefault();
            }
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Ninjas.Attach(ninja);
                context.Ninjas.Remove(ninja);
                // can also do this:
                // context.Entry(ninja).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        private static void DeleteNinjaWithStoredProc()
        {
            var keyval = 11;
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                context.Database.ExecuteSqlCommand("exec dbo.DeleteNinja {0}", keyval);
            }
        }

        private static void InsertNinjaWithEquipment()
        {
            using(var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = new Ninja
                {
                    Name = "George Costanza",
                    ServedInOniwaban = false,
                    DateOfBirth = new DateTime(1965, 1, 1),
                    ClanId = 1
                };
                var sword = new NinjaEquipment
                {
                    Name = "Art Vandalay Sword",
                    Type = EquipmentType.Weapon
                };
                var grappleHook = new NinjaEquipment
                {
                    Name = "Art Vandalay Grappling Hook",
                    Type = EquipmentType.Tool
                };
                // context.Ninjas.Add(ninja);
                ninja.EquipmentOwned.Add(sword);
                ninja.EquipmentOwned.Add(grappleHook);
                context.Ninjas.Add(ninja); // I like this better. This makes more sense to me
                context.SaveChanges();
            }
        }

        private static void SimpleNinjaGraphQueryEagerLoading()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = context.Ninjas
                    .Include(n => n.EquipmentOwned)
                    .Where(n => n.Name.StartsWith("George"))
                    .FirstOrDefault();
            }
        }

        private static void SimpleNinjaGraphQueryExplicitLoading()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = context.Ninjas
                    .Where(n => n.Name.StartsWith("George"))
                    .FirstOrDefault();

                Console.WriteLine("Ninja retreived: {0}", ninja.Name);
                context.Entry(ninja).Collection(n => n.EquipmentOwned).Load();

                foreach (var e in ninja.EquipmentOwned)
                {
                    Console.WriteLine(e.Name);
                }
            }
        }

        private static void SimpleNinjaGraphQueryLazyLoading()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;

                var ninja = context.Ninjas
                    .Where(n => n.Name.StartsWith("George"))
                    .FirstOrDefault();
                Console.WriteLine("Ninja retreived: {0}", ninja.Name);

                // Ninja.EquipmentOwned in class Ninja has to be marked as virutal to make this work
                foreach (var e in ninja.EquipmentOwned)
                {
                    Console.WriteLine(e.Name);
                }
            }
        }

        private static void NinjaProjectionQuery()
        {
            using (var context = new NinjaContext())
            {
                context.Database.Log = Console.WriteLine;
                var ninjas = context.Ninjas
                    .Select((n) => new { n.Name, n.DateOfBirth, n.EquipmentOwned })
                    .ToList();

                foreach (var ninja in ninjas)
                {
                    Console.WriteLine("{0}", ninja.);
                }

                var moreNinjas = context.Ninjas
                    .Select((n) => new { n.Name, n.DateOfBirth })
                    .ToList();
            }
        }
    }
}
