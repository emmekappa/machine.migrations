using Machine.Migrations;

public class RenameUsersIdColumn : FluentMigration
{
	public override void Up()
	{
		SimpleSchema.RenameColumn("Profiles", "Id", "ProfileId");
	}

	public override void Down()
	{
		SimpleSchema.RenameColumn("Profiles", "ProfileId", "Id");
	}
}