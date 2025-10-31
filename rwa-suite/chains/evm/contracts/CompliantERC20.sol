// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

interface IComplianceRegistry {
    function canTransfer(address from, address to) external view returns (bool);
}

contract CompliantERC20 is ERC20, Ownable {
    bool public paused;
    IComplianceRegistry public registry;

    constructor(string memory name_, string memory symbol_, address registry_) ERC20(name_, symbol_) Ownable(msg.sender) {
        registry = IComplianceRegistry(registry_);
    }

    function setPaused(bool p) external onlyOwner { paused = p; }
    function setRegistry(address r) external onlyOwner { registry = IComplianceRegistry(r); }

    function _update(address from, address to, uint256 value) internal override {
        require(!paused, "paused");
        if (from != address(0) && to != address(0)) {
            require(registry.canTransfer(from, to), "blocked");
        }
        super._update(from, to, value);
    }

    function mint(address to, uint256 amount) external onlyOwner { _mint(to, amount); }
}
