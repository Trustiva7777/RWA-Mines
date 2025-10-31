// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract ComplianceRegistry {
    mapping(address => bool) public allowlist;
    mapping(address => uint256) public lockupUntil; // unix timestamp

    address public owner;

    event AllowlistSet(address indexed who, bool allowed);
    event LockupSet(address indexed who, uint256 until);

    modifier onlyOwner() {
        require(msg.sender == owner, "not owner");
        _;
    }

    constructor() {
        owner = msg.sender;
    }

    function setAllowlist(address who, bool allowed) external onlyOwner {
        allowlist[who] = allowed;
        emit AllowlistSet(who, allowed);
    }

    function setLockup(address who, uint256 until) external onlyOwner {
        lockupUntil[who] = until;
        emit LockupSet(who, until);
    }

    function canTransfer(address from, address to) external view returns (bool) {
        if (!allowlist[from] || !allowlist[to]) return false;
        if (block.timestamp < lockupUntil[from]) return false;
        return true;
    }
}
