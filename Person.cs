using System.Text;
namespace sqlqb
{
   public class Person : SQLQueryBuilder.ITableInfo
   {
      public string table_name { get; private set; }
      public int id = -1;
      public string? first_name = null;
      public string? last_name = null;
      public int age = -1;
      //public DateTime? date_of_brith = null;

      public Person(int id, string first_name, string last_name, int age)
      {
         this.id = id;
         this.first_name = first_name;
         this.last_name = last_name;
         this.age = age;
         this.table_name = "person";
      }

      public override string ToString()
      {
         var sb = new StringBuilder($"first_name: {first_name}, ");
         sb.Append($"last_name: {last_name},").Append($" age: {age}");
         return sb.ToString();
      }
   }
}