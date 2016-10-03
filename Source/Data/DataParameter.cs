using System;
using System.Data;
using System.Data.Linq;
using System.Xml;
using System.Xml.Linq;
using Bars2Db.Mapping;

namespace Bars2Db.Data
{
    [ScalarType]
    public class DataParameter
    {
        public DataParameter()
        {
        }

        public DataParameter(string name, object value, DataType dataType)
        {
            Name = name;
            Value = value;
            DataType = dataType;
        }

        /// <summary>
        ///     Gets or sets the <see cref="T:Bars2Db.DataType" /> of the parameter.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:Bars2Db.DataType" /> values. The default is <see cref="F:Bars2Db.DataType.Undefined" />.
        /// </returns>
        public DataType DataType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the parameter is input-only, output-only, bidirectional, or a stored
        ///     procedure return value parameter.
        /// </summary>
        /// <returns>
        ///     One of the <see cref="T:System.Data.ParameterDirection" /> values. The default is Input.
        /// </returns>
        public ParameterDirection? Direction { get; set; }

        /// <summary>
        ///     Gets or sets the name of the <see cref="T:Bars2Db.Data.DataParameter" />.
        /// </summary>
        /// <returns>
        ///     The name of the <see cref="T:Bars2Db.Data.DataParameter" />. The default is an empty string.
        /// </returns>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <returns>
        ///     The maximum size, in bytes, of the data within the column. The default value is inferred from the parameter value.
        /// </returns>
        public int? Size { get; set; }

        /// <summary>
        ///     Gets or sets the value of the parameter.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Object" /> that is the value of the parameter. The default value is null.
        /// </returns>
        public object Value { get; set; }

    }
}