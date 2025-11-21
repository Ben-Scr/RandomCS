# RandomCS

## Usage
- Fast random value generation
- Cryptographically secure random number generation

## How to use

Non-Secure RNG
```csharp
using BenScr.Random;

RandomCS random = new RandomCS();
int randomInt = RandomHandler.NextInt(0, 10);
float randomFloat = RandomHandler.NextFloat(0.0f, 10.0f);
bool randomBool = random.NextBool();
string randomString = random.NextString();
```

Safe RNG
```csharp
using BenScr.Security.Cryptography;

RandomSecure random = new RandomSecure();
int randomInt = random.NextInt(0, 10);
float randomFloat = random.NextFloat(0.0f, 10.0f);
bool randomBool = random.NextBool();
string randomString = random.NextString();
```

Static Helper Class
```csharp
using BenScr.Security.Cryptography;

// Non-Secure
int randomInt = RandomHandler.NextInt(0, 10);
inr randomFloat = RandomHandler.NextFloat(0.0f, 10.0f);

// Secure
int randomInt = RandomHandler.Secure.NextInt(0, 10);
int randomFloat = RandomHandler.Secure.NextFloat(0.0f, 10.0f);
```

Generic Functions
```csharp
// Works with RandomCS, RandomSecure and RandomHandler
int randomInt = random.Next<int>(0, 10);
float randomFloat = random.Next<float>(0.0f, 10.0f);
bool randomBool = random.Next<bool>();
string randomString = random.Next<string>();
```
