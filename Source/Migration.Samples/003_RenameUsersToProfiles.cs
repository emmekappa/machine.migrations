using Machine.Migrations;

public class RenameUsersToProfiles : FluentMigration
{
	public override void Up()
	{
		SimpleSchema.RenameTable("Users", "Profiles");
	}

	public override void Down()
	{
		SimpleSchema.RenameTable("Profiles", "Users");
	}
}