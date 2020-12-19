using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.WPF;

namespace TestAutomation.Tide.DataBase
{
    /// <summary>
    /// 
    /// </summary>
    public class TableConstraint : DbView
    {
        public override string Name { get; set; }
        public override FontAwesomeIcon Icon { get; set; } = FontAwesomeIcon.List;
        public override DbType Type => DbType.Constraint;
        public string SchemaName { get; set; }
        public string TableView { get; set; }
        public string ObjectType { get; set; }
        public string ConstraintType { get; set; }
        public string Details { get; set; } 
    }
}