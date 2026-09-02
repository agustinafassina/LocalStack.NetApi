using LocalStack.Models.Dto;
using LocalStack.Repository.Interfaces;

namespace LocalStack.Repository.Implementations
{
    public class ItemRepository : IItemRepository
    {
        private readonly List<ItemDto> _items = new();

        public ItemRepository()
        {
            _items.Add(new ItemDto { Id = 1, Name = "Item 1" });
            _items.Add(new ItemDto { Id = 2, Name = "Item 2" });
        }

        public IEnumerable<ItemDto> GetAll() => _items;

        public ItemDto? GetById(int id) => _items.FirstOrDefault(i => i.Id == id);

        public ItemDto Add(ItemDto item)
        {
            bool newId = _items.Any() ? _items.Max(i => i.Id) + 1 : 1;
            var newItem = new ItemDto { Id = newId, Name = item.Name };
            _items.Add(newItem);
            return newItem;
        }
    }
}
