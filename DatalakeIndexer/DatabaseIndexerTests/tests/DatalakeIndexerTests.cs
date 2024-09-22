using DatabaseIndexer;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseIndexerTests.Tests;

public class DatalakeIndexerTests : DatabaseTest
{
    internal readonly PathRowType _mockPath = new PathRowType
    {
        CreatedOn = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        DeletedOn = null,
        ETag = "someetag",
        FilesystemName = "somefilesystem",
        LastModified = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        Path = "somepath"
    };

    [Test]
    public void UpsertPathsAsync_empty()
    {
        var indexer = TestServiceProvider.GetRequiredService<DatalakeIndexer>();

        var paths = new List<PathRowType>();

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable();

        Assert.That(actual.Count(), Is.EqualTo(0));
    }

    [Test]
    public void UpsertPathsAsync_returns_inserted()
    {
        var indexer = TestServiceProvider.GetRequiredService<DatalakeIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { Path = "somepath_1"},
            _mockPath with { Path = "somepath_2"},
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].Path, Is.EqualTo(paths[0].Path));
            Assert.That(actual[1].Path, Is.EqualTo(paths[1].Path));
        });
    }

    [Test]
    public void UpsertPathsAsync_returns_updated()
    {
        var indexer = TestServiceProvider.GetRequiredService<DatalakeIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { Path = "somepath_1"},
            _mockPath with { Path = "somepath_2"},
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].Path, Is.EqualTo(paths[0].Path));
            Assert.That(actual[1].Path, Is.EqualTo(paths[1].Path));
        });

        var updated = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Has.Count.EqualTo(2));
            Assert.That(updated[0].Path, Is.EqualTo(paths[0].Path));
            Assert.That(updated[1].Path, Is.EqualTo(paths[1].Path));
        });
    }

    [Test]
    public async Task UpsertPathsAsync_metadata_modified()
    {
        var indexer = TestServiceProvider.GetRequiredService<DatalakeIndexer>();

        var paths = new List<PathRowType>
        {
            _mockPath with { Path = "somepath_1"},
            _mockPath with { Path = "somepath_2"},
        };

        var actual = indexer.UpsertPathsAsync(paths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(2));
            Assert.That(actual[0].Path, Is.EqualTo(paths[0].Path));
            Assert.That(actual[1].Path, Is.EqualTo(paths[1].Path));
        });

        var metadata = new List<PathMetadataRowType>
        {
            new PathMetadataRowType
            {
                ETag = "someetag",
                MetadataJson = "{}",
                PathKey = paths[0].PathKey,
            },
            new PathMetadataRowType
            {
                ETag = "someetag",
                MetadataJson = "{}",
                PathKey = paths[1].PathKey,
            }
        };

        var actualMetadataCount = await indexer.UpsertPathsMetadataAsync(metadata);

        Assert.That(actualMetadataCount, Is.EqualTo(2));

        var updatedPaths = new List<PathRowType>
        {
            _mockPath with { Path = "somepath_1"},
            _mockPath with { Path = "somepath_2", ETag = "somemodifiedetag"},
        };

        var updated = indexer.UpsertPathsAsync(updatedPaths).ToBlockingEnumerable().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(updated, Has.Count.EqualTo(1));
            Assert.That(updated[0].Path, Is.EqualTo(updatedPaths[1].Path));
        });
    }
}
