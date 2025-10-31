// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

import "forge-std/Test.sol";
import "../src/Registry.sol";

contract RegistryTest is Test {
    function test_ok() public {
        Registry r = new Registry();
        assertTrue(r.ok());
    }
}
