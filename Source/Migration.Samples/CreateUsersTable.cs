using Machine.Migrations;

namespace Migration.Samples
{
	[Migration(1)]
	public class CreateUsersTable : FluentMigration
	{
		public override void Up() 
		{		
			Schema.CreateTable("Users", builder =>
			                                 	{
			                                 		builder.AddPrimaryKey<int>("Id").Identity();
			                                 		builder.AddColumn<string>("Name");			                                 		
			                                 	});
		}

		public override void Down()
		{
			SimpleSchema.DropTable("Users");			
		}
	}
}