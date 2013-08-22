using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq;

namespace xDev.Data
{
    /// <summary>
    /// Meta information for the entity.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public sealed class MetaInfo<T>
        where T : class, IEntity<T>, new()
    {
        #region [ Fields ]

        private readonly Type _entityType;
        private readonly TableMetaInfo _tableInfo;
        private readonly ReadOnlyDictionary<string, ColumnMetaInfo> _columnInfos;

        private readonly ReadOnlyCollection<string> _properties;
        private readonly ReadOnlyDictionary<string, Type> _propertyTypes;
        private readonly ReadOnlyCollection<string> _keys;
        private readonly ReadOnlyCollection<string> _navigationProperties;

        private readonly ReadOnlyDictionary<string, Delegate> _setters;
        private readonly ReadOnlyDictionary<string, Delegate> _getters;
        private readonly ReadOnlyDictionary<string, ReadOnlyCollection<ValidationAttribute>> _validationRules;

        private readonly Func<object[], T> _constructor;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public MetaInfo()
        {
            // Store entity type
            this._entityType = typeof(T);

            // Get meta info for the table
            this._tableInfo = GetTableInfo(this._entityType);

            // Get PropertyInfo for the type.
            var properties = this._entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Get property names
            this._properties = GetPropertyNames(properties);

            // Get property types
            this._propertyTypes = GetPropertyTypes(properties);

            // Get key property names
            this._keys = GetKeyPropertyNames(properties);

            // Get navigation property names
            this._navigationProperties = GetNavigationPropertyNames(properties);

            // Get meta info for the columns
            this._columnInfos = GetColumnsInfos(properties);

            // Get setters for the properties
            this._setters = GetPropertySetters(this._properties);

            // Get getters for the properties
            this._getters = GetPropertyGetters(this._properties);

            // Get all validations for the properties
            this._validationRules = GetPropertyValidationRules(properties);

            // Get the constructor delegate
            this._constructor = GetConstructor(this._properties, this._propertyTypes);
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public Type EntityType
        {
            get
            {
                return this._entityType;
            }
        }


        /// <summary>
        /// Gets the table meta info the entity is mapped to.
        /// </summary>
        public TableMetaInfo TableInfo
        {
            get
            {
                return this._tableInfo;
            }
        }


        /// <summary>
        /// Gets the meta info of the columns.
        /// </summary>
        public ReadOnlyDictionary<string, ColumnMetaInfo> ColumnInfos
        {
            get
            {
                return this._columnInfos;
            }
        }


        /// <summary>
        /// Gets list of entity property names.
        /// </summary>
        public ReadOnlyCollection<string> Properties
        {
            get
            {
                return this._properties;
            }
        }


        /// <summary>
        /// Gets types of the properties.
        /// </summary>
        public ReadOnlyDictionary<string, Type> PropertyTypes
        {
            get
            {
                return this._propertyTypes;
            }
        }


        /// <summary>
        /// Gets list of entity key property names.
        /// </summary>
        public ReadOnlyCollection<string> Keys
        {
            get
            {
                return this._keys;
            }
        }


        /// <summary>
        /// Gets list of entity navigation property names.
        /// </summary>
        public ReadOnlyCollection<string> NavigationProperties
        {
            get
            {
                return this._navigationProperties;
            }
        }


        /// <summary>
        /// Gets list of all property setters.
        /// </summary>
        internal ReadOnlyDictionary<string, Delegate> Setters
        {
            get
            {
                return this._setters;
            }
        }


        /// <summary>
        /// Gets list of all property getters.
        /// </summary>
        internal ReadOnlyDictionary<string, Delegate> Getters
        {
            get
            {
                return this._getters;
            }
        }


        /// <summary>
        /// Gets list of all property validations.
        /// </summary>
        internal ReadOnlyDictionary<string, ReadOnlyCollection<ValidationAttribute>> ValidationRules
        {
            get
            {
                return this._validationRules;
            }
        }

        #endregion

        
        #region [ Public Methods ]
        
        /// <summary>
        /// Creates new entity and initializes it with supplied properties.
        /// </summary>
        /// <param name="properties">Properties for the initialization.</param>
        /// <returns>Returns new entity of type <typeparamref name="T"/>.</returns>
        public T Create(params object[] properties)
        {
            if(this._constructor == null)
            {
                return null;
            }
            
            if(properties.Length > this._properties.Count)
            {
                throw new ArgumentOutOfRangeException("properties", string.Format("Unable to create and initialize entity of type {0}. The number of supplied properties is out of range.", this._entityType.FullName));
            }

            return this._constructor(properties);
        }


        /// <summary>
        /// Creates new entity and initializes it with supplied properties.
        /// </summary>
        /// <param name="propertyMappings">Properties for the initialization.</param>
        /// <returns>Returns new entity of type <typeparamref name="T"/>.</returns>
        public T Create(params Tuple<string, object>[] propertyMappings)
        {
            if (this._constructor == null)
            {
                return null;
            }

            if (propertyMappings.Length > this._properties.Count)
            {
                throw new ArgumentOutOfRangeException("properties", string.Format("Unable to create and initialize entity of type {0}. The number of supplied properties is out of range.", this._entityType.FullName));
            }

            // Check for invalid properties
            Func<Tuple<string, object>, bool> hasInvalidProperty = pm => this._properties.IndexOf(pm.Item1) == -1;
            if(propertyMappings.Any(hasInvalidProperty))
            {
                var invalidProp = propertyMappings.First(hasInvalidProperty);
                throw new InvalidOperationException(string.Format("Unable to create and initialize entity of type {0}. Supplied properties contains invalid property name '{1}'.", this._entityType.FullName, invalidProp.Item1));
            }

            // Prepare values
            var values = new List<object>(this._properties.Count);

            // Loop all properties
            foreach(string property in this._properties)
            {
                // Try to find this property
                var pMapping = propertyMappings.FirstOrDefault(pm => pm.Item1.Equals(property));

                // If there is not any value for the current property use null instead
                if(pMapping == null)
                {
                    values.Add(null);
                    continue;
                }

                // Store the supplied value
                values.Add(pMapping.Item2);
            }

            return Create(values.ToArray());
        }
        
        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Gets the table meta info the enitity is mapped to.
        /// </summary>
        /// <param name="type">Type of the entity.</param>
        /// <returns>Returns table meta info the enitity is mapped to.</returns>
        private static TableMetaInfo GetTableInfo(Type type)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            
            if(tableAttr == null)
            {
                throw new MissingAttributeException(typeof(TableAttribute), type, "Unable to get table meta info.");
            }
            
            return new TableMetaInfo(tableAttr.Name, tableAttr.Schema);
        }


        /// <summary>
        /// Gets meta info for all columns.
        /// </summary>
        /// <param name="properties">List of <see cref="T:System.Reflection.PropertyInfo"/> objects for the entity type <typeparamref name="T"/>.</param>
        /// <returns>Returns dictionary of the column meta infos.</returns>
        private ReadOnlyDictionary<string, ColumnMetaInfo> GetColumnsInfos(IEnumerable<PropertyInfo> properties)
        {
            var infos = new Dictionary<string, ColumnMetaInfo>(properties.Count());
            
            // Gets meta info for each property
            foreach(var property in this._properties)
            {
                // Get PropertyInfo
                var pInfo = properties.FirstOrDefault(p => p.Name.Equals(property) && p.IsDefined(typeof(ColumnAttribute)));
                if(pInfo == null)
                {
                    throw new MissingAttributeException(typeof(ColumnAttribute), this._entityType, string.Format(@"Unable to get column meta info for the property {0}.", property));    
                }

                // Get column attribute for the property
                var colAttr = pInfo.GetCustomAttribute<ColumnAttribute>();
                
                // Create meta info for the column
                var colInfo = new ColumnMetaInfo(colAttr.Name, colAttr.TypeName, colAttr.Order)
                {
                    IsKey = pInfo.IsDefined(typeof(KeyAttribute))
                };

                // Is column nullable ?
                colInfo.IsNullable = !colInfo.IsKey && !pInfo.IsDefined(typeof(RequiredAttribute));

                // Is empty string allowed?
                if(!colInfo.IsNullable && pInfo.IsDefined(typeof(RequiredAttribute)))
                {
                    colInfo.AllowEmptyStrings = pInfo.GetCustomAttribute<RequiredAttribute>().AllowEmptyStrings;
                }

                // Store column meta info
                infos.Add(property, colInfo);
            }

            return new ReadOnlyDictionary<string, ColumnMetaInfo>(infos);
        }


        /// <summary>
        /// Gets all property names.
        /// </summary>
        /// <param name="properties">List of <see cref="T:System.Reflection.PropertyInfo"/> objects for the entity type <typeparamref name="T"/>.</param>
        private static ReadOnlyCollection<string> GetPropertyNames(IEnumerable<PropertyInfo> properties)
        {
            return new ReadOnlyCollection<string>(properties
                .Where(p => p.CanRead && p.CanWrite && !p.GetMethod.IsVirtual)
                .Where(p => !p.IsDefined(typeof(AssociationAttribute)))
                .Select(p => p.Name)
                .ToList());
        }


        /// <summary>
        /// Gets types for each property.
        /// </summary>
        /// <param name="properties">List of <see cref="T:System.Reflection.PropertyInfo"/> objects for the entity type <typeparamref name="T"/>.</param>
        private static ReadOnlyDictionary<string, Type> GetPropertyTypes(IEnumerable<PropertyInfo> properties)
        {
            return new ReadOnlyDictionary<string, Type>(properties
                .Where(p => p.CanRead && p.CanWrite)
                .Select(p => p)
                .ToDictionary(p => p.Name, p => p.PropertyType));
        }


        /// <summary>
        /// Gets all key property names.
        /// </summary>
        /// <param name="properties">List of <see cref="T:System.Reflection.PropertyInfo"/> objects for the entity type <typeparamref name="T"/>.</param>
        private static ReadOnlyCollection<string> GetKeyPropertyNames(IEnumerable<PropertyInfo> properties)
        {
            return new ReadOnlyCollection<string>(properties
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => p.IsDefined(typeof(KeyAttribute)))
                .Where(p => !p.IsDefined(typeof(AssociationAttribute)))
                .Select(p => p.Name)
                .ToList());
        }


        /// <summary>
        /// Gets all navigation property names.
        /// </summary>
        /// <param name="properties">List of <see cref="T:System.Reflection.PropertyInfo"/> objects for the entity type <typeparamref name="T"/>.</param>
        private static ReadOnlyCollection<string> GetNavigationPropertyNames(IEnumerable<PropertyInfo> properties)
        {
            return new ReadOnlyCollection<string>(properties
                .Where(p => p.CanRead && p.CanWrite && p.GetMethod.IsVirtual)
                .Where(p => p.IsDefined(typeof(AssociationAttribute)))
                .Select(p => p.Name)
                .ToList());
        }


        /// <summary>
        /// Gets the property setter.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Returns setter for the property.</returns>
        private Delegate GetPropertySetter(string propertyName)
        {
            // Get expression for the entity type paramater
            var enityTypeExpr = Expression.Parameter(this._entityType);

            // Get the type of the property
            var propertyType = this._propertyTypes[propertyName];

            // Get expression for the property type parameter
            var propertyTypeExpr = Expression.Parameter(propertyType, propertyName);

            // Get property getter expression
            var propertyGetterExpr = Expression.Property(enityTypeExpr, propertyName);
            
            // Get the setter delegate
            var result = Expression.Lambda
            (
                Expression.Assign(propertyGetterExpr, propertyTypeExpr), enityTypeExpr, propertyTypeExpr
            ).Compile();

            return result;
        }


        /// <summary>
        /// Gets the property getter.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Returns getter for the property.</returns>
        private Delegate GetPropertyGetter(string propertyName)
        {
            // Get expression for the entity type paramater
            var enityTypeExpr = Expression.Parameter(this._entityType, "value");

            // Get property getter expression
            var propertyGetterExpr = Expression.Property(enityTypeExpr, propertyName);

            // Get the getter delegate
            var result = Expression.Lambda(propertyGetterExpr, enityTypeExpr).Compile();

            return result;
        }


        /// <summary>
        /// Gets all property setters.
        /// </summary>
        /// <param name="properties">Property names.</param>
        /// <returns>Returns setters for the properties.</returns>
        private ReadOnlyDictionary<string, Delegate> GetPropertySetters(IEnumerable<string> properties)
        {
            return new ReadOnlyDictionary<string, Delegate>(properties
                .ToDictionary(p => p, GetPropertySetter));
        }


        /// <summary>
        /// Gets all property getters.
        /// </summary>
        /// <param name="properties">Property names.</param>
        /// <returns>Returns getters for the properties.</returns>
        private ReadOnlyDictionary<string, Delegate> GetPropertyGetters(IEnumerable<string> properties)
        {
            return new ReadOnlyDictionary<string, Delegate>(properties
                .ToDictionary(p => p, GetPropertyGetter));
        }


        /// <summary>
        /// Gets all validation attributes for all entity properties.
        /// </summary>
        /// <param name="properties">List of entity's <see cref="T:System.Reflection.PropertyInfo"/> objects.</param>
        /// <returns>Returns disctionary of <see cref="T:System.ComponentModel.DataAnnotations.ValidationAttribute"/> object for each property.</returns>
        private ReadOnlyDictionary<string, ReadOnlyCollection<ValidationAttribute>> GetPropertyValidationRules(IEnumerable<PropertyInfo> properties)
        {
            var validations = new Dictionary<string, ReadOnlyCollection<ValidationAttribute>>(properties.Count());

            foreach(var propInfo in properties)
            {
                // Process only registered properties
                if(this._properties.IndexOf(propInfo.Name) == -1)
                {
                    continue;
                }

                // Get all validation attributes
                var validatioAttrs = propInfo.GetCustomAttributes()
                    .OfType<ValidationAttribute>()
                    .ToList();

                // If there are not any validations for the property continue with the next one
                if(validatioAttrs.Count <= 0)
                {
                    continue;
                }

                // Store validations
                validations.Add(propInfo.Name, new ReadOnlyCollection<ValidationAttribute>(validatioAttrs));
            }

            return new ReadOnlyDictionary<string, ReadOnlyCollection<ValidationAttribute>>(validations);
        }


        /// <summary>
        /// Creates entity constructor delegate.
        /// </summary>
        /// <param name="properties">List of entity's property names.</param>
        /// <param name="propertyTypes">List of entity's property types.</param>
        /// <returns>Returns constructor delegate.</returns>
        private Func<object[], T> GetConstructor(IList<string> properties, IDictionary<string, Type> propertyTypes)
        {
            // List of all property assignment bindings
            var bindings = new List<MemberAssignment>();

            // Expression for the constructor input arguments "params object[] @params"
            var paramsExpr = Expression.Parameter(typeof(object[]), "@params");

            // Expression for the "null"
            var nullExpr = Expression.Constant(null);

            // Create binding for each property
            for(int i = 0; i < properties.Count; i++)
            {
                // Get property name and type
                string pName = properties[i];
                Type pType = propertyTypes[pName];
                var pTypeExpr = Expression.Constant(pType, typeof(Type));

                // Expression for accessing constructor input parameter "@params[i]"
                var paramExpr = Expression.ArrayAccess(paramsExpr, Expression.Constant(i, typeof(int)));

                // Expresion for type changing "Convert.ChangeType(@params[i], int)
                var changeTypeExpr = Expression.Call(typeof(Convert), "ChangeType", null, paramExpr, pTypeExpr);

                // Expression for converting the input argument "(int)Convert.ChangeType(@params[i], int)
                var convertParamExpr = Expression.ConvertChecked(changeTypeExpr, pType);

                // Create binding expression "{Id = (@params[i] != null) ? (int)Convert.ChangeType(@params[0], int), default(int)}"
                var bindingExpr = Expression.Bind(this._entityType.GetProperty(pName), 
                    Expression.Condition(Expression.NotEqual(paramExpr, nullExpr)
                        , convertParamExpr
                        , Expression.Default(pType)));

                // Store the binding expression
                bindings.Add(bindingExpr);
            }

            // Create entity init expression
            var entityInitExpr = Expression.MemberInit(Expression.New(this._entityType), bindings);

            // Create lambda expression for the constructor
            var constructorExpr = Expression.Lambda<Func<object[], T>>(entityInitExpr, paramsExpr);

            return constructorExpr.Compile();
        }

        #endregion


        #region [ Static Methods ]

        /// <summary>
        /// Gets the meta information for the entity type.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <returns>Returns new instance of a <see cref="xDev.Data.MetaInfo{T}"/> class.</returns>
        public static MetaInfo<T> GetMetaInfo()
        {
            return new T().GetMetaInfo();
        }

        #endregion
    }
}
