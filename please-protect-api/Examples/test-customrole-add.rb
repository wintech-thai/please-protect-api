#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/CustomRole/org/#{orgId}/action/AddCustomRole"
param = {
  RoleName: "OWNER#2",
  RoleDescription: "Able to do anything",
  Tags: "testing",
  Permissions: [
    {
      ControllerName: "AccountDoc",
      ApiPermissions: [
        { ApiName: "GetAccountDocCashInvoices", ControllerName: "AccountDoc", IsAllowed: true },
      ]
    }
  ]
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
