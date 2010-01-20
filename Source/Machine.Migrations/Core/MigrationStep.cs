﻿using System;

namespace Machine.Migrations
{
  public class MigrationStep
  {
    readonly MigrationReference _migrationReference;
    readonly bool _reverting;
    IDatabaseMigration _databaseMigration;

    public MigrationReference MigrationReference
    {
      get { return _migrationReference; }
    }

    public IDatabaseMigration DatabaseMigration
    {
      get { return _databaseMigration; }
      set { _databaseMigration = value; }
    }

    public bool Reverting
    {
      get { return _reverting; }
    }

    public long Version
    {
      get { return _migrationReference.Version; }
    }

    public long VersionAfterApplying
    {
      get
      {
        if (_reverting)
        {
          return (short)(_migrationReference.Version - 1);
        }
        return _migrationReference.Version;
      }
    }

    public MigrationStep(MigrationReference migrationReference, bool reverting)
    {
      _migrationReference = migrationReference;
      _reverting = reverting;
    }

    public void Apply()
    {
      if (_reverting)
      {
        _databaseMigration.Down();
      }
      else
      {
        _databaseMigration.Up();
      }
    }

    public override bool Equals(object obj)
    {
      MigrationStep step = obj as MigrationStep;
      if (step == null) return false;
      return this.MigrationReference.Equals(step.MigrationReference) && this.Reverting == step.Reverting;
    }

    public override int GetHashCode()
    {
      return this.MigrationReference.GetHashCode();
    }

    public override string ToString()
    {
      return String.Format("{1}({0})", this.MigrationReference, this.Reverting ? "Down" : "Up");
    }
  }
}