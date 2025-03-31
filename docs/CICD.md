```markdown
# Basic Jest Test Setup

This guide shows how to quickly add Jest and a basic passing test to your project, resolving the `npm error Missing script: "test"` and providing a starting point for CI/CD.

**1. Install Jest:**

Install Jest as a development dependency:

```bash
npm install --save-dev jest
```

--save-dev, installs it only for development not for production

**2. Add Test Script to `package.json`:**

Open your `package.json` and add a `test` script to the `scripts` section:

```json
{
  // ... other package.json content ...
  "scripts": {
    // ... other scripts may be here ...
    "test": "jest"
  },
  // ... rest of package.json ...
}
```

**3. Create a Dummy Test File:**

Create a file like `src/__tests__/dummy.test.js` (or any name ending in `.test.js`):

```javascript
// src/dummy.test.js
describe('Placeholder Test Suite', () => {
  it('should always pass', () => {
    expect(true).toBe(true); // Simple assertion that always passes
  });
});
```

**4. Run Tests:**

Now you can run the tests using the command:

```bash
npm test
```

Jest will find the `test` script, execute, find your dummy test file, and report a passing test.
```