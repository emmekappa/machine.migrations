using System;
using Machine.Migrations;

namespace Migration.Samples
{
	public class CreateRolesTableAndRelationshipWithUsers : FluentMigration
	{
		public override void Up()
		{
			Schema.CreateTable("Roles", builder =>
			{
				builder.AddPrimaryKey<Guid>("Id");
				builder.AddColumn<string>("Name");
			});
			SimpleSchema.AddColumn("Users", "RoleId", typeof(Guid));
			SimpleSchema.AddForeignKeyConstraint("Users", "FK_Users_Roles", "RoleId", "Roles", "Id");
			Database.ExecuteNonQuery("INSERT INTO Users Name VALUES (?)", new[] {"Admin"});
			//Schema.AlterTable("Users", builder => builder.AddColumn<string>("RoleId"));
			//Schema.AlterTable("Users", builder => builder.AddForeignKey("RoleId",
			//                                                            new TableInfo("Roles", "Id", ColumnType.Guid, null)));
		}

		public override void Down()
		{
			SimpleSchema.DropTable("Roles");
			SimpleSchema.DropConstraint("Users", "FK_Users_Roles");
			SimpleSchema.RemoveColumn("Users", "RoleId");
		}
	}
}