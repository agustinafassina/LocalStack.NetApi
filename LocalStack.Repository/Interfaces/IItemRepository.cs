using LocalStack.Models.Dto;

namespace LocalStack.Repository.Interfaces
{
    public interface IItemRepository
    {
        IEnumerable<ItemDto> GetAll();
        ItemDto? GetById(int id);
        ItemDto Add(ItemDto item);
    }
}
