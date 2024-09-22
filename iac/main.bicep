param Location string = resourceGroup().location
param AppName string = 'dlg2db'

resource datalakeThingy 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'srchteststg${AppName}'
  location: Location
  sku: { name: 'Standard_ZRS' }
  kind: 'StorageV2'
  identity: { type: 'SystemAssigned' }
  properties: {
    isHnsEnabled: true
  }

  resource Identifier 'blobServices' = {
    name: 'default'

    resource Identifier 'containers' = {
      name: 'stuff'
    }
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${AppName}sb'
  location: Location
  sku: {
    name: 'Basic'
  }

  resource blobCreatedEventQueue 'queues' = {
    name: 'blob-created-event-queue'
    properties: {
      maxSizeInMegabytes: 2048
    }
  }

  resource blobDeletedEventQueue 'queues' = {
    name: 'blob-deleted-event-queue'
  }
}

resource systemTopic 'Microsoft.EventGrid/systemTopics@2023-06-01-preview' = {
  name: 'sometopic'
  location: Location
  properties: {
    source: datalakeThingy.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }

  resource blobCreatedToQueueSubscription 'eventSubscriptions' = {
    name: 'blobCreatedToQueueSubscription'
    properties: {
      destination: {
        properties: {
          resourceId: serviceBus::blobCreatedEventQueue.id
        }
        endpointType: 'ServiceBusQueue'
      }
      filter: {
        includedEventTypes: [
          'Microsoft.Storage.BlobCreated'
        ]
      }
    }
  }

  resource blobDeletedToQueueSubscription 'eventSubscriptions' = {
    name: 'blobDeletedToQueueSubscription'
    properties: {
      destination: {
        properties: {
          resourceId: serviceBus::blobDeletedEventQueue.id
        }
        endpointType: 'ServiceBusQueue'
      }
      filter: {
        includedEventTypes: [
          'Microsoft.Storage.BlobDeleted'
        ]
      }
    }
  }
}
