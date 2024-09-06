namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        ILikeRepository LikeRepository { get; }
        IPostRepository PostRepository { get; }
        IPhotoRepository PhotoRepository{ get; }
        Task<bool> Complete();
        bool HasChanges();
    }
}