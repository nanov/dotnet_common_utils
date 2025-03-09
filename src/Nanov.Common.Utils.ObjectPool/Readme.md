# Nanov.Common.Utils.ObjectPool

A high-performance, generic object pooling library for .NET applications that need efficient object reuse.

## Overview

This library provides two object pool implementations:

1. `ObjectPool<T, TStrategy, TConstructParams>`: A simple, non-thread-safe object pool
2. `ConcurrentObjectPool<T, TStrategy, TConstructParams>`: A thread-safe object pool for concurrent access

Both implementations allow for custom object creation, preparation, and cleanup strategies through the `IObjectPoolStrategy<T, TConstructParams>` interface.

## Features

- Generic implementation supporting any reference type
- Customizable object lifecycle management through strategies
- Thread-safe concurrent implementation
- Efficient memory usage with tunable capacity
- Support for object parameters during construction and rental
- Value-based rental option with automatic return semantics

## Installation

```
dotnet add package Nanov.Common.Utils.ObjectPool
```

## Usage

### Defining a Strategy

First, implement the object pool strategy:

```csharp
public readonly struct MyObjectStrategy : IObjectPoolStrategy<MyObject, MyParams>
{
    public MyObject Create(MyParams param)
    {
        // Create a new instance based on parameters
        return new MyObject(param.Id);
    }

    public void Prepare(MyObject obj, MyParams param)
    {
        // Reset or prepare the object for reuse
        obj.Reset(param.Id);
    }

    public void Clean(MyObject obj)
    {
        // Clean up the object before returning to pool
        obj.Clear();
    }
}
```

### Using the Standard Object Pool

```csharp
// Create a pool with a capacity of 100 objects
var pool = new ObjectPool<MyObject, MyObjectStrategy, MyParams>(
    new MyObjectStrategy(), 
    maxCapacity: 100
);

// Rent an object
var obj = pool.Rent(new MyParams { Id = 42 });

// Use the object...

// Return the object to the pool
pool.Return(obj);
```

### Using the Concurrent Object Pool

```csharp
// Create a thread-safe pool with a capacity of 100 objects
var concurrentPool = new ConcurrentObjectPool<MyObject, MyObjectStrategy, MyParams>(
    new MyObjectStrategy(), 
    maxCapacity: 100
);

// Rent an object from multiple threads
var obj = concurrentPool.Rent(new MyParams { Id = 42 });

// Use the object...

// Return the object to the pool
concurrentPool.Return(obj);
```

### Using RentedValue for Automatic Returns

```csharp
// Scoped rental that automatically returns the object
using (pool.RentValue(new MyParams { Id = 42 }, out var obj))
{
    // Use the object...
    // Automatically returned when exiting the scope
}
```

## Performance Considerations

### ObjectPool

- Fastest for single-threaded scenarios
- Zero allocation after pool initialization
- Constant-time performance for rent and return operations
- Best for high-frequency, single-threaded usage

### ConcurrentObjectPool

- Thread-safe with minimal locking
- Uses compare-and-swap operations for synchronization
- Adaptive storage strategy for improved throughput
- Slightly higher overhead than the non-concurrent version

## Implementation Details

### ObjectPool

The standard `ObjectPool` uses a simple array-based storage mechanism with an index pointer. It provides O(1) rent and return operations but is not thread-safe.

### ConcurrentObjectPool

The `ConcurrentObjectPool` employs a two-tier storage strategy:
- A "pocket" container for the fastest access
- A dynamically growing pool for additional storage
- Thread-safe operations using `Interlocked` methods
- Auto-adjusting capacity based on usage patterns

## License

[MIT License](LICENSE)