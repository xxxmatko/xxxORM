using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;

namespace xDev.Data
{
    /// <summary>
    /// Extension methods for the <see cref="IEntity{T}"/>.
    /// </summary>
    public static class IEntityExtensions
    {
        #region [ Static Fields ]

        /// <summary>
        /// Internal lock for <see cref="IEntityExtensions.MetaInfos"/> property.
        /// </summary>
        internal static object SyncRoot;


        /// <summary>
        /// Cache for the entity metainformations.
        /// </summary>
        internal static Dictionary<Type, WeakReference> MetaInfos;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Static constructor.
        /// </summary>
        static IEntityExtensions()
        {
            IEntityExtensions.SyncRoot = new object();
            IEntityExtensions.MetaInfos = new Dictionary<Type, WeakReference>();
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Gets the meta information for the entity.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="this">Instance of an entity.</param>
        /// <returns>Returns new instance of a <see cref="xDev.Data.MetaInfo{T}"/> class.</returns>
        public static MetaInfo<T> GetMetaInfo<T>(this IEntity<T> @this) 
            where T : class, IEntity<T>, new()
        {
            var entityType = typeof(T);
            MetaInfo<T> info = null;

            lock(IEntityExtensions.SyncRoot)
            {
                // Get WeakReference for the enityt type meta info
                var infoRef = IEntityExtensions.MetaInfos.ContainsKey(entityType) ? IEntityExtensions.MetaInfos[entityType] : null;

                if((infoRef != null) && infoRef.IsAlive)
                {
                    info = infoRef.Target as MetaInfo<T>;
                }
                else
                {
                    // If there is not meta info for the desired type clear all dead references
                    var infoRefKeys = IEntityExtensions.MetaInfos
                        .Where(miRef => (miRef.Value == null) || !miRef.Value.IsAlive)
                        .Select(miRef => miRef.Key);

                    // Remove all dead referencies
                    foreach(var key in infoRefKeys)
                    {
                        IEntityExtensions.MetaInfos.Remove(key);
                    }
                }

                // If there is not any meta info create new one
                if(info == null)
                {
                    info = new MetaInfo<T>();
                    IEntityExtensions.MetaInfos[entityType] = new WeakReference(info);
                }
            }

            return info;
        }


        /// <summary>
        /// Sets value of the property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="this">Instance of an entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Value which has to be set.</param>
        public static void SetValue<T>(this IEntity<T> @this, string propertyName, object value)
            where T : class, IEntity<T>, new()
        {
            // Get meta info for the entity
            var mi = @this.GetMetaInfo();
            
            // Try to set property value
            SetValueInternal(@this, propertyName, value, mi);
        }


        /// <summary>
        /// Gets value of the property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="this">Instance of an entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Value of the property.</param>
        public static object GetValue<T>(this IEntity<T> @this, string propertyName)
            where T : class, IEntity<T>, new()
        {
            // Get meta info for the entity
            var mi = @this.GetMetaInfo();

            // Get the property value
            return GetValueInternal(@this, propertyName, mi);
        }


        /// <summary>
        /// Gets value which indicates whether the entity is valid or not.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="this">Instance of an entity.</param>
        /// <returns>Returns <c>true</c> if the entity is valid, <c>false</c> otherwise.</returns>
        public static bool IsValid<T>(this IEntity<T> @this)
            where T : class, IEntity<T>, new()
        {
            // Get meta info for the entity
            var mi = @this.GetMetaInfo();

            // If there are not any validations, enitity is valid
            if(!mi.ValidationRules.Any())
            {
                return true;    
            }

            // Try to find first invalid property
            foreach(var propertyValidationRules in mi.ValidationRules)
            {
                // Create ValidationContext
                var context = new ValidationContext(@this)
                {
                    DisplayName = propertyValidationRules.Key,
                    MemberName = propertyValidationRules.Key
                };

                // Get property value which has to be validate
                var value = GetValueInternal(@this, propertyValidationRules.Key, mi);

                // Try to find first invalid rule for the current property
                foreach(var rule in propertyValidationRules.Value)
                {
                    // Get the ValidationResult
                    var result = rule.GetValidationResult(value, context);

                    // If the validation result is not null than this property is invalid
                    if (result != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Gets list of <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> objects 
        /// which represent validation state of the entity. Gets only the first invalid rule for each property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="this">Instance of an entity.</param>
        /// <returns>Returns list of <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> objects.</returns>
        public static IEnumerable<ValidationResult> Validate<T>(this IEntity<T> @this)
            where T : class, IEntity<T>, new()
        {
            // Get meta info for the entity
            var mi = @this.GetMetaInfo();

            // If there are not any validations, enitity is valid
            if (!mi.ValidationRules.Any())
            {
                return Enumerable.Empty<ValidationResult>();
            }

            // Get validation results for the enitity
            return ValidateInternal(@this, mi);
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Sets value of the property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">Instance of an entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Value which has to be set.</param>
        /// <param name="metaInfo">Entity meta info.</param>
        private static void SetValueInternal<T>(IEntity<T> entity, string propertyName, object value, MetaInfo<T> metaInfo)
            where T : class, IEntity<T>, new()
        {
            // Check if the property name is valid
            if (!metaInfo.Properties.Contains(propertyName))
            {
                throw new ArgumentException(string.Format(@"Unable to dynamicly set value for the property {0}. Property does not exist on the object instance.", propertyName), "propertyName");
            }

            // Check if there is setter for the property
            if (!metaInfo.Setters.ContainsKey(propertyName))
            {
                throw new ArgumentException(string.Format(@"Unable to dynamicly set value for the property {0}. There is not any setter for the property.", propertyName), "propertyName");
            }

            try
            {
                // Try to set property value
                metaInfo.Setters[propertyName].DynamicInvoke(entity, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(@"Unable to dynamicly set value for the property {0}.", propertyName), ex);
            }
        }


        /// <summary>
        /// Gets value of the property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">Instance of an entity.</param>
        /// <param name="metaInfo">Entity meta info.</param>
        /// <returns>Returns list of <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> objects.</returns>
        private static object GetValueInternal<T>(IEntity<T> entity, string propertyName, MetaInfo<T> metaInfo)
            where T : class, IEntity<T>, new()
        {
            // Check if the property name is valid
            if (!metaInfo.Properties.Contains(propertyName))
            {
                throw new ArgumentException(string.Format(@"Unable to dynamicly get value for the property {0}. Property does not exist on the object instance.", propertyName), "propertyName");
            }

            // Check if there is getter for the property
            if (!metaInfo.Getters.ContainsKey(propertyName))
            {
                throw new ArgumentException(string.Format(@"Unable to dynamicly get value for the property {0}. There is not any getter for the property.", propertyName), "propertyName");
            }

            try
            {
                // Try to get property value
                return metaInfo.Getters[propertyName].DynamicInvoke(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(@"Unable to dynamicly get value for the property {0}.", propertyName), ex);
            }
        }


        /// <summary>
        /// Gets list of <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> objects 
        /// which represent validation state of the entity. Gets only the first invalid rule for each property.
        /// </summary>
        /// <typeparam name="T">Type of the entity.</typeparam>
        /// <param name="entity">Instance of an entity.</param>
        /// <param name="metaInfo">Entity meta info.</param>
        /// <returns>Returns list of <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> objects.</returns>
        private static IEnumerable<ValidationResult> ValidateInternal<T>(IEntity<T> entity, MetaInfo<T> metaInfo)
            where T : class, IEntity<T>, new()
        {
            // Validate all properties
            foreach (var propertyValidationRules in metaInfo.ValidationRules)
            {
                // Create ValidationContext
                var context = new ValidationContext(entity)
                {
                    DisplayName = propertyValidationRules.Key,
                    MemberName = propertyValidationRules.Key
                };

                // Get property value which has to be validate
                var value = GetValueInternal(entity, propertyValidationRules.Key, metaInfo);

                // Find invalid rule for the current property
                foreach (var rule in propertyValidationRules.Value)
                {
                    // Get the ValidationResult
                    var result = rule.GetValidationResult(value, context);

                    // If the validation result is not null than this property is invalid
                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }

        #endregion
    }
}
