using Machine.Migrations;

public class CreateUsersTable : FluentMigration
{
	public override void Up()
	{
		Schema.CreateTable("Users", builder =>
		                            	{
											//builder.AddPrimaryKey<int>("Id").Identity();
											//builder.AddPrimaryKey<int>("Id").Sequence("hibernate_sequence");
											builder.AddPrimaryKey<int>("Id").Native();
											builder.AddColumn<string>("Name");
											builder.AddColumn<string>("LastName");
		                            	});
		//SimpleSchema.AddColumn("Users", "RoleId", typeof(Guid), true);
		//SimpleSchema.RenameColumn("Users", "RoleId", "RoleIds");
		//SimpleSchema.RemoveColumn("Users", "RoleIds");
		//SimpleSchema.RenameTable("Users", "Usersssss");
		//SimpleSchema.RenameTable("Usersssss", "Users");
		//SimpleSchema.AddUniqueConstraint("Users", "Name", "Name");
		//SimpleSchema.DropConstraint("Users", "Name");
		//SimpleSchema.DropTable("Users");
		SimpleSchema.AddIndex("Users", "Users", true, true, "Name", "LastName");
		//SimpleSchema.DropIndex("Users", "Users");
	}

	public override void Down()
	{
		SimpleSchema.DropTable("Users");
	}
}