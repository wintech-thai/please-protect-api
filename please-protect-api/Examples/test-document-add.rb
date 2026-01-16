#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Document/org/#{orgId}/action/AddDocument"
param = {
  DocName: "TestDoc#8",
  Description: "Test job create for document add",
  Tags: "testing3",
  DocType: "OAXRG",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
