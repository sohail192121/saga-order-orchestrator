# Prototype boundaries

An execute action that performs a side effect and then throws may need its own idempotent cleanup. Compensation only applies to actions that returned successfully. Durable state and message deduplication are pending.
