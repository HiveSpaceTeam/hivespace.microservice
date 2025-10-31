using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductAttribute : Entity<int>
    {
        #region Properties
        public int AttributeId { get; private set; }
        public Guid ProductId { get; private set; }

        private readonly List<int> _selectedValueIds = [];
        public IReadOnlyCollection<int> SelectedValueIds => _selectedValueIds.AsReadOnly();

        public string? FreeTextValue { get; private set; }
        #endregion

        #region Constructors
        // Parameterless constructor for Entity Framework
        private ProductAttribute()
        {
        }

        public ProductAttribute(int attributeId, Guid productId, IEnumerable<int>? selectedValueIds = null, string? freeTextValue = null)
        {
            AttributeId = attributeId;
            ProductId = productId;
            if (selectedValueIds is not null) _selectedValueIds.AddRange(selectedValueIds);
            FreeTextValue = string.IsNullOrWhiteSpace(freeTextValue) ? null : freeTextValue.Trim();
            if (IsInvalid())
            {
                throw new InvalidAttributeException();
            }
        }


        #endregion

        #region Methods
        private bool IsInvalid()
        {
            return AttributeId == 0
                || ProductId == Guid.Empty
                || (_selectedValueIds.Count == 0 && string.IsNullOrWhiteSpace(FreeTextValue));
        }

        public void SetSelectedValues(IEnumerable<int> selectedValueIds)
        {
            _selectedValueIds.Clear();
            if (selectedValueIds is not null)
            {
                _selectedValueIds.AddRange(selectedValueIds);
            }
            if (IsInvalid())
            {
                throw new InvalidAttributeException();
            }
        }

        public void AddSelectedValue(int valueId)
        {
            if (valueId == 0) return;
            if (!_selectedValueIds.Contains(valueId))
            {
                _selectedValueIds.Add(valueId);
            }
        }

        public void RemoveSelectedValue(int valueId)
        {
            if (valueId == 0) return;
            _selectedValueIds.Remove(valueId);
            if (IsInvalid())
            {
                throw new InvalidAttributeException();
            }
        }

        public void SetFreeTextValue(string? value)
        {
            FreeTextValue = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            if (IsInvalid())
            {
                throw new InvalidAttributeException();
            }
        }

        #endregion
    }
}
