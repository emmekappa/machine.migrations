using Machine.Migrations;

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