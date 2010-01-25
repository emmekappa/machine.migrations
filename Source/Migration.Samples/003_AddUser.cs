using Machine.Migrations;

public class AddUser : FluentMigration
{
	public override void Up()
	{
		Database.ExecuteNonQuery("INSERT INTO Users (Name) VALUES ('{0}')", new[] {"Admin"});
	}

	public override void Down()
	{
		Database.ExecuteNonQuery("DELETE FROM Users WHERE Name = '{0}'", new[] { "Admin" });
	}
}