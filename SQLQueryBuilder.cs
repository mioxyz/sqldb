using System;
using System.Data.SqlClient;

namespace sqlqb {
   public class SQLQueryBuilder {

      public interface ITableInfo {
         string table_name { get; }
      }

      private static readonly Type typeString = typeof(String);
      private static readonly Type typeBool = typeof(bool);
      private static readonly Type typeInt32 = typeof(Int32);
      private static readonly Type typeInt64 = typeof(Int64);
      // ... TODO

      public class Reader<T> {
         public SqlConnection connection;
         public SqlDataReader reader;

         private Type type_;
         private readonly Type[] parameters;
         private readonly SqlCommand command;

         public Reader(SqlConnection connection, string query) {
            this.connection = connection;
            connection.Open();

            type_ = typeof(T);

            command = new SqlCommand(query, connection);

            reader = command.ExecuteReader();
            var fields = type_.GetFields().ToList();
            var parameters = new List<Type>();
            foreach (var field in fields) {
               //Console.WriteLine(field.Name);
               parameters.Add(field.FieldType);
            }
            this.parameters = parameters.ToArray();
         }

         public T? read() {
            if (!reader.Read()) return default(T);

            object[] realizedParameters = new object[parameters.Length];
            var k = 0;
            //Console.WriteLine("before parameter loop");
            foreach (var parameter in parameters) {
               //Console.WriteLine(parameter.Name);
               if (parameter == typeBool) {
                  realizedParameters[k] = reader.GetBoolean(k);
               }
               else if (parameter == typeString) {
                  realizedParameters[k] = reader.GetString(k);
               }
               else if (parameter == typeInt32) {
                  realizedParameters[k] = reader.GetInt32(k);
               }
               else {
                  throw new NotImplementedException(parameter.ToString());
               }
               ++k;
            }
            var constructor = type_.GetConstructor(parameters);
            var something = (T)constructor.Invoke(realizedParameters);
            if (null == something) return default(T);
            return (T)something;
         }
      }

      // public class Inserter<T> {
      //    public SqlConnection connection;
      //    public SqlDataReader reader;
      // }

      private bool defaultInitialized = false;
      public static Dictionary<string, string> connectionStrings = new Dictionary<string, string>();
      public const string DEFAULT_CONNECTION_STRING_NAME = "__DEFAULT__";
      // fill the dictionary as we go. possibly add option to initialize the query templates
      // before execution of main application. We don't need it, since we can pre-generate
      // everything the same way we do for select, infact, we should make the same dictionary
      // for the constructor for select<T> and add it as a on-the-fly compiled lambda function.
      public static Dictionary<string, string> queryTemplateInsert = new Dictionary<string, string>();
      public static Dictionary<string, (string, Type)[]> objectParameterTypeNamePairs = new Dictionary<string, (string, Type)[]>();

      public static void setConnectionString(string connectionString, string? name = null) {
         connectionStrings[(null == name) ? DEFAULT_CONNECTION_STRING_NAME : name] = connectionString;
      }

      public static Reader<T> select<T>(string query, string connectionStringName = DEFAULT_CONNECTION_STRING_NAME) {
         var connection = new SqlConnection(connectionStrings[connectionStringName]);
         return new Reader<T>(connection, query);
      }

      public static void register<T>(string tableName) where T : ITableInfo {
         Type? type_ = typeof(T);

         if (Nullable.GetUnderlyingType(type_) != null) {
            type_ = Nullable.GetUnderlyingType(type_);
         }
         if(type_ == null) throw new Exception("type cannot be null");
         if(type_.FullName == null) throw new Exception("full name cannot be null");
         string typeName = type_.FullName;

         var fields = type_.GetFields();
         var parameters = new List<(string, Type)>();
         var sb = new System.Text.StringBuilder();
         for(var k = 0; k < fields.Length; ++k) {
            //Console.WriteLine(field.Name);
            parameters.Add((fields[k].Name, fields[k].FieldType));
            if(fields[k].Name == "id") continue;
            sb.Append(fields[k].Name);
            if(k != fields.Length - 1) sb.Append(", ");
         }
         objectParameterTypeNamePairs[typeName] = parameters.ToArray();

         if (!queryTemplateInsert.ContainsKey(typeName)) {
            //var sb_ = new System.Text.StringBuilder("INSERT INTO ");
            //sb_.Append(tableName).Append(" VALUES (");
            queryTemplateInsert[typeName] = $"INSERT INTO {tableName}({sb.ToString()}) VALUES ";
         }
      }

      private static string generateInsertRow<T>(T obj, string typeName, Type type_) {
         var sb = new System.Text.StringBuilder(" (");
         var count = 0;
         //TODO probably better to use a normal for loop.
         foreach (var (name, t) in objectParameterTypeNamePairs[typeName]) {
            if(name == "id") {
               ++count;
               continue;
            }
            Console.WriteLine($"trying to get property with name: {name}");
            Console.WriteLine($"of type_.Name: {type_.Name}");
            // TODO IMPORTANT !!!!!! we are iterating 
            // over a list of (string, Type) instead of an Array of FieldInfo.
            // FieldInfos is basically a struct which includes both the field name and its type
            // so there is no need build that list like that, we can just use it directly.
            var fieldInfo = type_.GetField(name);
            if (fieldInfo == null) throw new Exception("TODO");
            object? propertyValue = fieldInfo.GetValue(obj);
            if(propertyValue == null) throw new Exception("propertyValue cannot be null.");
            if (t == typeBool) {
               sb.Append(((bool)propertyValue) ? "1" : "0");
            } else if (t == typeString) {
               sb.Append("'").Append(propertyValue).Append("'");
            } else if (t == typeInt32) {
               sb.Append(propertyValue);
            } else {
               throw new NotImplementedException($"not implemented: {name}.");
            }
            if (count++ != objectParameterTypeNamePairs[typeName].Length - 1) sb.Append(", ");
         }
         sb.Append(")");
         return sb.ToString();
      }

      public static bool insert<T>(T obj, string connectionStringName = DEFAULT_CONNECTION_STRING_NAME)
      where T : ITableInfo {

         Type? type_ = typeof(T);
         if (Nullable.GetUnderlyingType(type_) != null) {
            type_ = Nullable.GetUnderlyingType(type_);
         }

         if(type_.FullName == null) throw new Exception("type_.FullName cannot be null.");

         string typeName = type_.FullName;

         if (!queryTemplateInsert.ContainsKey(typeName)) {
            throw new Exception("you need to register that type first.");
            // var sb_ = new System.Text.StringBuilder("INSERT INTO ");
            // sb_.Append(obj.table_name).Append(" VALUES (");
            // queryTemplateInsert[typeName] = $"INSERT INTO {obj.table_name}(TODO cols) VALUES ";
         }
         
         Console.WriteLine("-----------");
         Console.WriteLine(queryTemplateInsert[typeName]);

         var sb = new System.Text.StringBuilder(queryTemplateInsert[typeName]);
         // insert row, we can abstract this at a later point when we're doing multi-row inserts.
         sb.Append(generateInsertRow<T>(obj, typeName, type_));

         var connection = new SqlConnection(connectionStrings[connectionStringName]);
         connection.Open();
         var query = sb.ToString();
         Console.WriteLine("query:\n" + query);
         var command = new SqlCommand(query, connection);
         command.ExecuteNonQuery();
         connection.Close();

         return false;
      }


   }
}
