#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
ns = 'pp-development'
workload = 'pp-dev-pp-api'

#apiUrl = "api/Proxy/org/#{orgId}/action/Kube/apis/apps/v1/deployments" #ดึงทุก deployment ใน cluster
#apiUrl = "api/Proxy/org/#{orgId}/action/Kube/apis/apps/v1/namespaces/#{ns}/deployments/#{workload}" #ดึงข้อมูลเฉพาะ deployment นั้น ๆ
#apiUrl = "api/Proxy/org/#{orgId}/action/Kube/api/v1/namespaces" #ดึง namespaces ออกมาดู
apiUrl = "api/Proxy/org/#{orgId}/action/Kube/api/v1/pods" #ดึง pods ออกมาดู

param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
