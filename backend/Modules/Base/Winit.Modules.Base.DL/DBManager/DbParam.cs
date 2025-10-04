using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Base.DL.DBManager
{
    public class DbParam
    {
        private object _ParamValue;

        /// <summary>
        /// default constructor for DbParam class
        /// </summary>
        public DbParam() { }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramType">Parameter Type</param>
        public DbParam(string paramName, object paramValue, DbType paramType)
            : this()
        {
            this.ParamDirection = ParameterDirection.Input;
            this.ParamName = paramName;
            this.ParamType = paramType;
            this.ParamValue = paramValue;
        }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramSourceColumn">Parameter Source Columns</param>
        /// <param name="paramType">Parameter Type</param>
        public DbParam(string paramName, string paramValue, string paramSourceColumn, DbType paramType)
            : this(paramName, paramValue, paramType)
        {
            this.ParamSourceColumn = paramSourceColumn;
        }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramType">Parameter Type</param>
        /// <param name="paramDirection">Parameter Direction</param>
        public DbParam(string paramName, string paramValue, DbType paramType, ParameterDirection paramDirection)
            : this(paramName, paramValue, paramType)
        {
            this.ParamDirection = paramDirection;
        }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramType">Parameter Type</param>
        /// <param name="paramDirection">Parameter Direction</param>
        public DbParam(string paramName, object paramValue, DbType paramType, ParameterDirection paramDirection)
            : this(paramName, paramValue, paramType)
        {
            this.ParamDirection = paramDirection;
        }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramType">Parameter Type</param>
        /// <param name="paramDirection">Parameter Direction</param>
        public DbParam(string paramName, object paramValue, DbType paramType, string udtTypeName, ParameterDirection paramDirection)
            : this(paramName, paramValue, paramType, paramDirection)
        {
            this.UdtTypeName = udtTypeName;
        }

        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramSourceColumn">Parameter Source Columns</param>
        /// <param name="paramType">Parameter Type</param>
        /// <param name="paramDirection">Parameter Direction</param>
        /// <param name="Size">Size</param>
        public DbParam(string paramName, string paramValue, string paramSourceColumn, DbType paramType, ParameterDirection paramDirection, int Size)
            : this(paramName, paramValue, paramType, paramDirection)
        {
            this.ParamSourceColumn = paramSourceColumn;
            this.Size = Size;
        }
        /// <summary>
        /// overloaded constructor for DbParam class
        /// </summary>
        /// <param name="paramName">Parameter Name</param>
        /// <param name="paramValue">Parameter Value</param>
        /// <param name="paramSourceColumn">Parameter Source Columns</param>
        /// <param name="paramType">Parameter Type</param>
        /// <param name="paramDirection">Parameter Direction</param>
        /// <param name="Size">Size</param>
        public DbParam(string paramName, string paramValue, string paramSourceColumn, DbType paramType, ParameterDirection paramDirection)
            : this(paramName, paramValue, paramType, paramDirection)
        {
            this.ParamSourceColumn = paramSourceColumn;
        }
        /// <summary>
        /// gets or sets the parameter name
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// gets or sets the parameter value
        /// </summary>
        public object ParamValue
        {
            get { return _ParamValue; }
            set
            {
                if (value == null)
                {
                    _ParamValue = DBNull.Value;
                }
                else
                {
                    _ParamValue = value;
                }
            }
        }

        /// <summary>
        /// gets or sets the parameter source column
        /// </summary>
        public string ParamSourceColumn { get; set; }

        /// <summary>
        /// gets or sets the parameter type
        /// </summary>
        public DbType ParamType { get; set; }

        /// <summary>
        /// gets or sets the parameter direction
        /// </summary>
        public ParameterDirection ParamDirection { get; set; }

        /// <summary>
        /// gets or sets the UdtTypeName
        /// </summary>
        public string UdtTypeName { get; set; }

        /// <summary>
        /// Get or set Size
        /// </summary>
        public int Size { get; set; }
    }
}
