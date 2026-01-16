# please-protect-apis

* To migration
- dotnet ef migrations add <xxxx>
  - Example : dotnet ef migrations add Initial_001

* To remove migration
- dotnet ef migrations remove

INSERT INTO "ApiKeys" (key_id, api_key, org_id, key_created_date, key_expired_date, key_description, roles_list)
VALUES (gen_random_uuid(), gen_random_uuid(), 'default', current_timestamp, NULL, 'Default org owner', 'OWNER');



