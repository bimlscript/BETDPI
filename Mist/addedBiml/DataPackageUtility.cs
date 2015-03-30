using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Varigence.Flow.FlowFramework;
using Varigence.Flow.FlowFramework.Validation;

[Serializable]
public class DataPackage
{
	[DataMember]
	public string Name { get; set; }
	
	[DataMember]
	public ICollection<DataPackageResource> Resources { get; set; }	
	
	[DataMember]
	public string License { get; set; }
	
	public static DataPackage Load(string json)
	{
		var serializer = new JavaScriptSerializer();
		var dataPackage = serializer.Deserialize<DataPackage>(json);
		return dataPackage;
	}
	
	public void Store(string filePath)
	{
		var serializer = new JavaScriptSerializer();
		var json = serializer.Serialize(this);
		
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));
		File.WriteAllText(filePath, json);		
	}
	
	public ValidationReporter Validate()
	{
		var reporter = new ValidationReporter();
		ValidationHelper.ValidateRequiredProperty(reporter, "Name", Name);
		
		foreach (var resource in Resources)
		{
			resource.Validate(reporter);
		}
		
		return reporter;
	}
}

[Serializable]
public class DataPackageResource
{
	[DataMember]
	public string Url { get; set; }
	
	[DataMember]
	public string Path { get; set; }
	
	[DataMember]
	public DataPackageResourceSchema Schema { get; set; }
	
	public void Validate(ValidationReporter reporter)
	{
		ValidationHelper.ValidateRequiredProperty(reporter, "Path", Path);
		if (Schema == null)
		{
			reporter.Report(Severity.Error, "Schema descriptor must be supplied");
		}
	}
}

[Serializable]
public class DataPackageResourceSchema
{
	[DataMember]
	public ICollection<DataPackageResourceSchemaField> Fields { get; set; }
	
	public void Validate(ValidationReporter reporter)
	{
		if (Fields == null || Fields.Count == 0)
		{
			reporter.Report(Severity.Error, "At least one field must be supplied");
		}
		else
		{
			foreach (var field in Fields)
			{
				field.Validate(reporter);
			}
		}
	}
}

[Serializable]
public class DataPackageResourceSchemaField
{
	[DataMember]
	public string Name { get; set; }

	/// <summary>
	/// The type of the field (string, number etc) - see below for more detail. If type is not provided a consumer should assume a type of "string".
	/// </summary>
	[DataMember]
	public string Type { get; set; }
	
	/// <summary>
	/// A nicer human readable label or title for the field
	/// </summary>
	[DataMember]
	public string Title { get; set; }

	/// <summary>
	/// A description for this field e.g. "The recipient of the funds"
	/// </summary>
	[DataMember]
	public string Description { get; set; }

	/// <summary>
	/// A description of the format e.g. “DD.MM.YYYY” for a date. See below for more detail.
	/// </summary>
	[DataMember]
	public string Format { get; set; }

	/// <summary>
	/// A constraints descriptor that can be used by consumers to validate field values
	/// </summary>
	[DataMember]
	public DataPackageResourceSchemaFieldConstraints Constraints { get; set; }
	
	[ScriptIgnore]
	public int Length
	{
		get
		{
			if (DbType == DbType.String)
			{
				return 4000;
			}
			
			return 0;
		}
	}
	
	[ScriptIgnore]
	public int Precision
	{
		get
		{
			if (DbType == DbType.Decimal)
			{
				return 18;
			}
			
			return -1;
		}
	}
	
	[ScriptIgnore]
	public int Scale
	{
		get
		{
			if (DbType == DbType.Decimal)
			{
				return 0;
			}
			
			return -1;
		}
	}
	
	[ScriptIgnore]
	public DbType DbType
	{
		get
		{
			if (string.IsNullOrEmpty(Type)) { return DbType.String; }
			
			var cleanType = Type.Trim().ToLowerInvariant();
			switch (cleanType)
			{
				case "boolean":  return DbType.Boolean;
				case "integer":  return DbType.Int32;
				case "number":   return DbType.Decimal;
				case "string":   return DbType.String;
				case "datetime": return DbType.DateTime2;
				case "date":     return DbType.Date;
				case "time":     return DbType.Time;
				default:         throw new NotSupportedException("The Type '" + Type + "' is not supported presently.");
			}
		}
		set
		{
			switch (value)
			{
				case DbType.String:
					Type = "string";
					break;
				case DbType.Int16:
				case DbType.Int32:
				case DbType.Int64:
					Type ="integer";
					break;
				case DbType.Currency:
				case DbType.Decimal:
					Type = "number";
					break;
				case DbType.Boolean:
					Type = "boolean";
					break;
				case DbType.Date:
					Type = "date";
					break;
				case DbType.Time:
					Type = "time";
					break;
				case DbType.DateTime:
				case DbType.DateTime2:
					Type = "datetime";
					break;
				default:
					Type = "string";
					break;
					//throw new NotSupportedException("We Don't Support that type yet");
			}
		}
	}
	
	public void Validate(ValidationReporter reporter)
	{
		ValidationHelper.ValidateRequiredProperty(reporter, "Name", Name);
		ValidationHelper.ValidateRequiredProperty(reporter, "Type", Type, new[] { "string", "integer", "number", "boolean", "datetime", "date", "time" }, new[] { "null", "object", "array", "geopoint", "geojson", "any" });
	}
}

public class DataPackageResourceSchemaFieldConstraints
{
	/// <summary>
	/// A boolean value which indicates whether a field must have a value in every row of the table. An empty string is considered to be a missing value.
	/// </summary>
	[DataMember]
	public bool? Required { get; set; }
	
	/// <summary>
	/// An integer that specifies the minimum number of characters for a value
	/// </summary>
	[DataMember]
	public int? MinLength { get; set; }
	
	/// <summary>
	/// An integer that specifies the maximum number of characters for a value
	/// </summary>
	[DataMember]
	public int? MaxLength { get; set; }
	
	/// <summary>
	/// A boolean. If true, then all values for that field MUST be unique within the data file in which it is found. This defines a unique key for a row although a row could potentially have several such keys.
	/// </summary>
	[DataMember]
	public bool? Unique { get; set; }
	
	/// <summary>
	/// A regular expression that can be used to test field values. If the regular expression matches then the value is valid. Values will be treated as a string of characters. It is recommended that values of this field conform to the standard XML Schema regular expression syntax. See also this reference.
	/// </summary>
	[DataMember]
	public string Pattern { get; set; }
	
	/// <summary>
	/// specifies a minimum value for a field. This is different to minLength which checks number of characters. A minimum value constraint checks whether a field value is greater than or equal to the specified value. The range checking depends on the type of the field. E.g. an integer field may have a minimum value of 100; a date field might have a minimum date. If a minimum value constraint is specified then the field descriptor MUST contain a type key
	/// </summary>
	[DataMember]
	public string Minimum { get; set; }

	/// <summary>
	/// as above, but specifies a maximum value for a field.
	/// </summary>
	[DataMember]
	public string Maximum { get; set; }
}

public static class ValidationHelper
{
	public static void ValidateRequiredProperty(ValidationReporter reporter, string propertyName, string value)
	{
		if (Varigence.Utility.Extensions.StringChecker.IsNullOrWhiteSpace(value))
		{
			reporter.Report(Severity.Error, "Required property '{0}' was not supplied", propertyName);
		}
	}
	
	public static void ValidateRequiredProperty(ValidationReporter reporter, string propertyName, string value, IEnumerable<string> supportedValues, IEnumerable<string> unsupportedValues)
	{
		if (Varigence.Utility.Extensions.StringChecker.IsNullOrWhiteSpace(value))
		{
			reporter.Report(Severity.Error, "Required property '{0}' was not supplied", propertyName);
		}
		else if (!supportedValues.Contains(value))
		{
			if (unsupportedValues != null && unsupportedValues.Contains(value))
			{
				reporter.Report(Severity.Error, "The value '{0}' is not supported for the required property '{1}'", value, propertyName);
			}
			else
			{
				reporter.Report(Severity.Error, "The value '{0}' is not a valid value for the required property '{1}'", value, propertyName);
			}
		}
	}	
}
