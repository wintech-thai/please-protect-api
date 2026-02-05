#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'd8417099-8738-4004-8223-e61301a8effa'

apiUrl = "api/CustomRole/org/#{orgId}/action/UpdateCustomRoleById/#{id}"

param = {
  RoleName: "OWNER#2",
  RoleDescription: "Able to do anything XXX",
  Tags: "testing",
  Permissions: [
    {
      ControllerName: "AccountDoc",
      ApiPermissions: [
        { ApiName: "GetAccountDocCashInvoices", ControllerName: "AccountDoc", IsAllowed: true },
        { ApiName: "AddAccountDocCashInvoice",  ControllerName: "AccountDoc", IsAllowed: true },
      ]
    }
  ]
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
