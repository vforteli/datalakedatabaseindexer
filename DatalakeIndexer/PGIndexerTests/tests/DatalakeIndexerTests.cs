using Microsoft.Extensions.DependencyInjection;
using PGDatabaseIndexer;

namespace PGIndexerTests.Tests;

public class DatalakeIndexerTests : DatabaseTest
{
    internal readonly PathRowType _mockPath = new PathRowType
    {
        created_on = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        deleted_on = null,
        etag = "someetag",
        filesystem_name = "somefilesystem",
        last_modified = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        path = "somepath"
    };

    [Test]
    public void UpsertPathsAsync_empty()
    {
        var indexer = TestServiceProvider.GetRequiredService<PGServerIndexer>();

        var paths = new List<PathRowType>();

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable();

        Assert.That(actual.Count(), Is.EqualTo(0));
    }

    [Test]
    public void UpsertPathsAsync_returns_inserted()
    {
        var indexer = TestServiceProvider.GetRequiredService<PGServerIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { path = "somepath_1" },
            _mockPath with { path = "somepath_2" },
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].path, Is.EqualTo(paths[0].path));
            Assert.That(actual[1].path, Is.EqualTo(paths[1].path));
        });
    }

    [Test]
    public void UpsertPathsAsync_returns_updated()
    {
        var indexer = TestServiceProvider.GetRequiredService<PGServerIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { path = "somepath_1" },
            _mockPath with { path = "somepath_2" },
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].path, Is.EqualTo(paths[0].path));
            Assert.That(actual[1].path, Is.EqualTo(paths[1].path));
        });

        var updated = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Has.Count.EqualTo(2));
            Assert.That(updated[0].path, Is.EqualTo(paths[0].path));
            Assert.That(updated[1].path, Is.EqualTo(paths[1].path));
        });
    }

    [Test]
    public async Task UpsertPathsAsync_metadata_modified()
    {
        var indexer = TestServiceProvider.GetRequiredService<PGServerIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { path = "somepath_1" },
            _mockPath with { path = "somepath_2" },
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].path, Is.EqualTo(paths[0].path));
            Assert.That(actual[1].path, Is.EqualTo(paths[1].path));
        });

        var metadata = new List<PathMetadataRowType>
        {
            new PathMetadataRowType
            {
                etag = "someetag",
                metadata_json = "{}",
                path_key = paths[0].path_key,
            },
            new PathMetadataRowType
            {
                etag = "someetag",
                metadata_json = "{}",
                path_key = paths[1].path_key,
            }
        };

        var actualMetadataCount = await indexer.UpsertPathsMetadataAsync(metadata);

        Assert.That(actualMetadataCount, Is.EqualTo(2));

        var updatedPaths = new List<PathRowType>
        {
            _mockPath with { path = "somepath_1" },
            _mockPath with { path = "somepath_2", etag = "somemodifiedetag" },
        };

        var updated = indexer.UpsertPathsAsync(updatedPaths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Has.Count.EqualTo(1));
            Assert.That(updated[0].path, Is.EqualTo(updatedPaths[1].path));
        });
    }
}