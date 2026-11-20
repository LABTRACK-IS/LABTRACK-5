using LabTrack.Infrastructure.Persistence.DbContext;

namespace LabTrack.Infrastructure.Persistence.Repositories;

internal static class RepositoryExecutionEngine
{
    public static Task<T> ResolveAsync<T>(T value) => new ValueTask<T>(value).AsTask();

    public static T[] Empty<T>() => new T[0];

    public static Task CompleteAsync() => default(ValueTask).AsTask();

    public static Task TrackUpdateAsync<TEntity>(LabTrackDbContext db, TEntity entity) where TEntity : class
    {
        db.Set<TEntity>().Update(entity);
        return default(ValueTask).AsTask();
    }

    public static Task TrackDeleteAsync<TEntity>(LabTrackDbContext db, TEntity entity) where TEntity : class
    {
        db.Set<TEntity>().Remove(entity);
        return default(ValueTask).AsTask();
    }
}
