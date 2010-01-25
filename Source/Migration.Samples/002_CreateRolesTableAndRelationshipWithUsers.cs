using System;
using Machine.Migrations;

public class CreateRolesTableAndRelationshipWithUsers : FluentMigration
{
	public override void Up()
	{
		Schema.CreateTable("Roles", builder =>
		                            	{
		                            		builder.AddPrimaryKey<Guid>("Id");
		                            		builder.AddColumn<string>("Name");
		                            	});
		SimpleSchema.AddColumn("Users", "RoleId", typeof (Guid), true);
		SimpleSchema.AddForeignKeyConstraint("Users", "FK_Users_Roles", "RoleId", "Roles", "Id");		
	}

	public override void Down()
	{
		SimpleSchema.DropConstraint("Users", "FK_Users_Roles");
		SimpleSchema.DropTable("Roles");
		SimpleSchema.RemoveColumn("Users", "RoleId");
	}
}