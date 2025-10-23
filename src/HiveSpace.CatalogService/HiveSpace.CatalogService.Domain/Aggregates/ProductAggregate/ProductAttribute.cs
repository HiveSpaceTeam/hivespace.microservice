using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using System;
using System.Collections.Generic;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate
{
    public class ProductAttribute : Entity<Guid>
    {
        #region Properties
        public Guid AttributeId { get; private set; }
        public Guid ProductId { get; private set; }

        private readonly List<Guid> _selectedValueIds = [];
        public IReadOnlyCollection<Guid> SelectedValueIds => _selectedValueIds.AsReadOnly();

        public string? FreeTextValue { get; private set; }
        #endregion

        #region Constructors
        // Parameterless constructor for Entity Framework
        private ProductAttribute()
        {
        }

        public ProductAttribute(Guid attributeId, Guid productId, IEnumerable<Guid>? selectedValueIds = null, string? freeTextValue = null)
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
            return AttributeId == Guid.Empty
                || ProductId == Guid.Empty
                || (_selectedValueIds.Count == 0 && string.IsNullOrWhiteSpace(FreeTextValue));
        }

        public void SetSelectedValues(IEnumerable<Guid> selectedValueIds)
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

        public void AddSelectedValue(Guid valueId)
        {
            if (valueId == Guid.Empty) return;
            if (!_selectedValueIds.Contains(valueId))
            {
                _selectedValueIds.Add(valueId);
            }
        }

        public void RemoveSelectedValue(Guid valueId)
        {
            if (valueId == Guid.Empty) return;
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
