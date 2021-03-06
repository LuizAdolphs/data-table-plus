# Data Table Plus for .NET
Data Table Plus provides some extensions in order to transform object lists into data tables based on the object mappings (EntityFramework mappings) and also some sql helpers which perform some batch operations using the previously built data tables. This application is focused on solving some EF performance issues while ingesting or updating a lot of data (represented as objects).

# Dependencies

- EntityFramework >= 6.2.0 
- ServiceStack.Text >= 5.1.0

# Getting Started

- Configure the EF and the ConnectionString in the App.config:

```XML

<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false"/>
	</configSections>
	<connectionStrings>
		<add name="Context" providerName="System.Data.SqlClient" connectionString="YourConnectionString"/>
	</connectionStrings>
	<entityFramework>
		<defaultConnectionFactory type="System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework">
		</defaultConnectionFactory>
		<providers>
			<provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer"/>
		</providers>
	</entityFramework>
</configuration>

```

- Create a new database table:

```PLSQL

	CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Email] [varchar](150) NOT NULL,
	[Password] [varchar](255) NOT NULL,
 	CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	GO
  
 ```

- Create a new data model and a new mapping configuration that represent the database table:

```C#

    public partial class User
    {
        public User()
        { }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
  
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserId);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(250)
                .IsRequired();

            this.Property(t => t.Email)
                .HasMaxLength(150)
                .IsRequired();

            this.Property(t => t.Password)
                .HasMaxLength(255)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("User");
            this.Property(t => t.UserId).HasColumnName("UserId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Password).HasColumnName("Password");

            // Relationships
        }
    }
  
```   
      
- Add this configuration to the DbContext configurations:

```C#

  	internal class Context : DbContext
	{
		static Context()
		{
			Database.SetInitializer<Context>(null);
		}

		public Context()
			: this("Name=Context")
		{
		}

		protected Context(string connectionStringName)
			: base(connectionStringName)
		{
			this.Configuration.LazyLoadingEnabled = false;
			this.Configuration.ProxyCreationEnabled = false;
			this.Configuration.AutoDetectChangesEnabled = false;
			this.Configuration.ValidateOnSaveEnabled = false;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{			
			modelBuilder.Configurations.Add(new UserMap());
		}
	}
  
```
  
- Code Examples:

```C#
	
		[TestMethod]
		public void GeneralTestMethod()
		{
			var context = new Context();

			Startup.AddDbContext(context);
			
			// And/Or
			
			var connectionString = "YourConnectionString";
			
			Startup.AddConnectionString(connectionString);

			var entities = new List<User>
			{
				new User { Name = "John Doe", Email = "johndoe@gmail.com", Password = "rH&n&}eEB7!v5d&}" },
				new User { Name = "Jane Doe", Email = "janedoe@gmail.com", Password = "Jv7K,6zsv-4Bz7w}" },
				new User { Name = "Baby Doe", Email = "babydoe@gmail.com", Password = "f7{7LEVeTrJ-M~>." }
			};
			
			// Or you can construct your DataTable by another way
			var dataTable = entities.AsStronglyTypedDataTable();

			using (var sqlService = new SqlService())
			{
				sqlService.BulkInsert(dataTable);

				sqlService.BatchUpdate(dataTable, "Update [User] SET [Name] = 'Batch Update Usage Example' WHERE UserId = @UserId");
			}
		}
  
```
